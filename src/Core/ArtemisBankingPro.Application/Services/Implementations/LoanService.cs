using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Loans;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class LoanService : ILoanService
{
    private const decimal HighRiskAverageDebtThreshold = 500_000m;

    private readonly ILoanRepository _loanRepository;
    private readonly ISavingsAccountRepository _savingsAccountRepository;
    private readonly ITransactionService _transactionService;

    public LoanService(
        ILoanRepository loanRepository,
        ISavingsAccountRepository savingsAccountRepository,
        ITransactionService transactionService)
    {
        _loanRepository = loanRepository;
        _savingsAccountRepository = savingsAccountRepository;
        _transactionService = transactionService;
    }

    public async Task<int> AssignLoanAsync(AssignLoanDto dto)
    {
        if (await _loanRepository.HasActiveLoanAsync(dto.ApplicationUserId))
        {
            throw new DomainException("Client already has an active loan.");
        }

        var averageDebt = await CalculateAverageDebtAsync(dto.ApplicationUserId);

        if (averageDebt > HighRiskAverageDebtThreshold)
        {
            throw new HighRiskClientException("Client is classified as high risk based on average debt.");
        }

        var principalAccount = await _savingsAccountRepository.GetPrincipalAccountAsync(dto.ApplicationUserId)
            ?? throw new DomainException("Client does not have an active principal account.");

        var monthlyPayment = CalculateMonthlyPayment(dto.Amount, dto.AnnualInterestRate, dto.TermMonths);
        var loanNumber = await GenerateUniqueLoanNumberAsync();

        var loan = new Loan
        {
            LoanNumber = loanNumber,
            ApplicationUserId = dto.ApplicationUserId,
            Amount = dto.Amount,
            AnnualInterestRate = dto.AnnualInterestRate,
            TermMonths = dto.TermMonths,
            MonthlyPayment = monthlyPayment,
            Status = LoanStatus.Active,
            Installments = GenerateAmortizationSchedule(dto.Amount, dto.AnnualInterestRate, dto.TermMonths, monthlyPayment)
        };

        await _loanRepository.AddAsync(loan);
        await _loanRepository.SaveChangesAsync();

        await _transactionService.RegisterTransactionAsync(
            principalAccount.Id, TransactionType.Credit, dto.Amount, $"Loan disbursement - {loanNumber}");

        return loan.Id;
    }

    public async Task UpdateRateAsync(int loanId, decimal newAnnualRate)
    {
        var loan = await _loanRepository.GetWithInstallmentsAsync(loanId)
            ?? throw new NotFoundException(nameof(Loan), loanId);

        var pendingInstallments = loan.Installments
            .Where(i => i.Status == InstallmentStatus.Pending)
            .OrderBy(i => i.Number)
            .ToList();

        if (pendingInstallments.Count == 0)
        {
            throw new DomainException("Loan has no pending installments to recalculate.");
        }

        var remainingPrincipal = pendingInstallments.Sum(i => i.PrincipalAmount);
        var newMonthlyPayment = CalculateMonthlyPayment(remainingPrincipal, newAnnualRate, pendingInstallments.Count);

        var monthlyRate = newAnnualRate / 100 / 12;
        var balance = remainingPrincipal;

        foreach (var installment in pendingInstallments)
        {
            var interest = Math.Round(balance * monthlyRate, 2);
            var principal = Math.Round(newMonthlyPayment - interest, 2);

            installment.InterestAmount = interest;
            installment.PrincipalAmount = principal;
            installment.TotalAmount = newMonthlyPayment;

            balance -= principal;
        }

        loan.AnnualInterestRate = newAnnualRate;
        loan.MonthlyPayment = newMonthlyPayment;

        _loanRepository.Update(loan);
        await _loanRepository.SaveChangesAsync();
    }

    public async Task<PagedResult<LoanDto>> GetLoansAsync(
        string? status, string? cedula, int pageNumber, int pageSize)
    {
        var query = _loanRepository.Query()
            .Include(l => l.ApplicationUser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LoanStatus>(status, true, out var statusEnum))
        {
            query = query.Where(l => l.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(cedula))
        {
            query = query.Where(l => l.ApplicationUser.Cedula.Contains(cedula));
        }

        var totalCount = await query.CountAsync();

        var loans = await query
            .OrderBy(l => l.LoanNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = loans.Select(l => MapToDto(l, includeInstallments: false)).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<LoanDto?> GetLoanByIdAsync(int loanId)
    {
        var loan = await _loanRepository.GetWithInstallmentsAsync(loanId);

        return loan is null ? null : MapToDto(loan, includeInstallments: true);
    }

    public async Task PayInstallmentAsync(int loanId, int savingsAccountId, decimal amount)
    {
        var loan = await _loanRepository.GetWithInstallmentsAsync(loanId)
            ?? throw new NotFoundException(nameof(Loan), loanId);

        var pendingInstallments = loan.Installments
            .Where(i => i.Status is InstallmentStatus.Pending or InstallmentStatus.PartiallyPaid or InstallmentStatus.Overdue)
            .OrderBy(i => i.Number)
            .ToList();

        if (pendingInstallments.Count == 0)
        {
            throw new DomainException("Loan has no pending installments.");
        }

        var totalDue = pendingInstallments.Sum(i => i.TotalAmount - i.PaidAmount);

        if (amount > totalDue)
        {
            throw new DomainException("Payment amount exceeds total pending debt. Overpayment is not allowed.");
        }

        await _transactionService.RegisterTransactionAsync(
            savingsAccountId, TransactionType.Debit, amount, $"Loan payment - {loan.LoanNumber}");

        var remaining = amount;

        foreach (var installment in pendingInstallments)
        {
            if (remaining <= 0)
            {
                break;
            }

            var due = installment.TotalAmount - installment.PaidAmount;
            var applied = Math.Min(due, remaining);

            installment.PaidAmount += applied;
            installment.Status = installment.PaidAmount >= installment.TotalAmount
                ? InstallmentStatus.Paid
                : InstallmentStatus.PartiallyPaid;

            if (installment.Status == InstallmentStatus.Paid)
            {
                installment.PaidAt = DateTime.UtcNow;
            }

            remaining -= applied;
        }

        if (loan.Installments.All(i => i.Status == InstallmentStatus.Paid))
        {
            loan.Status = LoanStatus.PaidOff;
        }

        _loanRepository.Update(loan);
        await _loanRepository.SaveChangesAsync();
    }

    private async Task<decimal> CalculateAverageDebtAsync(string applicationUserId)
    {
        var activeLoans = await _loanRepository.GetActiveLoansByUserAsync(applicationUserId, LoanStatus.Active);

        return activeLoans.Count == 0 ? 0 : activeLoans.Average(l => l.Amount);
    }

    private static decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int termMonths)
    {
        var monthlyRate = annualRate / 100 / 12;

        if (monthlyRate == 0)
        {
            return Math.Round(principal / termMonths, 2);
        }

        var factor = (double)Math.Pow((double)(1 + monthlyRate), termMonths);
        var payment = principal * monthlyRate * (decimal)factor / ((decimal)factor - 1);

        return Math.Round(payment, 2);
    }

    private static List<LoanInstallment> GenerateAmortizationSchedule(
        decimal principal, decimal annualRate, int termMonths, decimal monthlyPayment)
    {
        var monthlyRate = annualRate / 100 / 12;
        var balance = principal;
        var installments = new List<LoanInstallment>();

        for (var i = 1; i <= termMonths; i++)
        {
            var interest = Math.Round(balance * monthlyRate, 2);
            var principalPortion = i == termMonths ? balance : Math.Round(monthlyPayment - interest, 2);

            installments.Add(new LoanInstallment
            {
                Number = i,
                DueDate = DateTime.UtcNow.AddMonths(i),
                PrincipalAmount = principalPortion,
                InterestAmount = interest,
                TotalAmount = principalPortion + interest,
                PaidAmount = 0,
                Status = InstallmentStatus.Pending
            });

            balance -= principalPortion;
        }

        return installments;
    }

    private static LoanDto MapToDto(Loan loan, bool includeInstallments) => new()
    {
        Id = loan.Id,
        LoanNumber = loan.LoanNumber,
        ApplicationUserId = loan.ApplicationUserId,
        OwnerFullName = loan.ApplicationUser is not null
            ? $"{loan.ApplicationUser.FirstName} {loan.ApplicationUser.LastName}"
            : string.Empty,
        OwnerCedula = loan.ApplicationUser?.Cedula ?? string.Empty,
        Amount = loan.Amount,
        AnnualInterestRate = loan.AnnualInterestRate,
        TermMonths = loan.TermMonths,
        MonthlyPayment = loan.MonthlyPayment,
        Status = loan.Status.ToString(),
        Installments = includeInstallments
            ? loan.Installments.OrderBy(i => i.Number).Select(i => new LoanInstallmentDto
            {
                Number = i.Number,
                DueDate = i.DueDate,
                PrincipalAmount = i.PrincipalAmount,
                InterestAmount = i.InterestAmount,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Status = i.Status.ToString()
            }).ToList()
            : []
    };

    private async Task<string> GenerateUniqueLoanNumberAsync()
    {
        string loanNumber;

        do
        {
            loanNumber = Random.Shared.NextInt64(100_000_000, 999_999_999).ToString();
        }
        while (await _loanRepository.LoanNumberExistsAsync(loanNumber));

        return loanNumber;
    }

    public async Task<LoanDto?> GetActiveLoanByUserIdAsync(string applicationUserId)
    {
        var loans = await _loanRepository.GetActiveLoansByUserAsync(applicationUserId, LoanStatus.Active);
        var loan = loans.FirstOrDefault();

        if (loan is null)
        {
            return null;
        }

        var withInstallments = await _loanRepository.GetWithInstallmentsAsync(loan.Id);

        return withInstallments is null ? null : MapToDto(withInstallments, includeInstallments: true);
    }
}