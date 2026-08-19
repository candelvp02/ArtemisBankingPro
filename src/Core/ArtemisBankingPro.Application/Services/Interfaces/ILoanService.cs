using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Loans;

namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface ILoanService
{
    Task<int> AssignLoanAsync(AssignLoanDto dto);
    Task UpdateRateAsync(int loanId, decimal newAnnualRate);
    Task<PagedResult<LoanDto>> GetLoansAsync(string? status, string? cedula, int pageNumber, int pageSize);
    Task<LoanDto?> GetLoanByIdAsync(int loanId);
    Task PayInstallmentAsync(int loanId, int savingsAccountId, decimal amount);
    Task<LoanDto?> GetLoanByNumberAsync(string loanNumber);
    Task<LoanDto?> GetActiveLoanByUserIdAsync(string applicationUserId);
}