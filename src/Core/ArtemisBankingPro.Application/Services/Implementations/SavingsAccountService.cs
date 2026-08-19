using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.SavingsAccounts;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class SavingsAccountService : ISavingsAccountService
{
    private readonly ISavingsAccountRepository _savingsAccountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionService _transactionService;

    public SavingsAccountService(
        ISavingsAccountRepository savingsAccountRepository,
        ITransactionRepository transactionRepository,
        ITransactionService transactionService)
    {
        _savingsAccountRepository = savingsAccountRepository;
        _transactionRepository = transactionRepository;
        _transactionService = transactionService;
    }

    public async Task<int> CreatePrincipalAccountAsync(string applicationUserId, decimal initialAmount)
    {
        var accountNumber = await GenerateUniqueAccountNumberAsync();

        var account = new SavingsAccount
        {
            AccountNumber = accountNumber,
            ApplicationUserId = applicationUserId,
            Type = AccountType.Principal,
            Status = AccountStatus.Active,
            Balance = initialAmount
        };

        await _savingsAccountRepository.AddAsync(account);
        await _savingsAccountRepository.SaveChangesAsync();

        if (initialAmount > 0)
        {
            var transaction = new Transaction
            {
                SavingsAccountId = account.Id,
                Type = TransactionType.Credit,
                Amount = initialAmount,
                BalanceAfter = account.Balance,
                Description = "Initial deposit on account opening"
            };

            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();
        }

        return account.Id;
    }

    public async Task<int> CreateSecondaryAccountAsync(CreateSecondaryAccountDto dto)
    {
        var principalAccount = await _savingsAccountRepository.GetPrincipalAccountAsync(dto.ApplicationUserId)
            ?? throw new DomainException("Client does not have an active principal account.");

        var accountNumber = await GenerateUniqueAccountNumberAsync();

        var account = new SavingsAccount
        {
            AccountNumber = accountNumber,
            ApplicationUserId = dto.ApplicationUserId,
            Type = AccountType.Secondary,
            Status = AccountStatus.Active,
            Balance = dto.InitialAmount
        };

        await _savingsAccountRepository.AddAsync(account);
        await _savingsAccountRepository.SaveChangesAsync();

        if (dto.InitialAmount > 0)
        {
            var transaction = new Transaction
            {
                SavingsAccountId = account.Id,
                Type = TransactionType.Credit,
                Amount = dto.InitialAmount,
                BalanceAfter = account.Balance,
                Description = "Initial deposit on secondary account opening"
            };

            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();
        }

        return account.Id;
    }

    public async Task CancelAccountAsync(int accountId)
    {
        var account = await _savingsAccountRepository.GetByIdAsync(accountId)
            ?? throw new NotFoundException(nameof(SavingsAccount), accountId);

        if (account.Type != AccountType.Secondary)
        {
            throw new DomainException("Only secondary accounts can be cancelled.");
        }

        if (account.Status != AccountStatus.Active)
        {
            throw new DomainException("Account is already cancelled.");
        }

        if (account.Balance > 0)
        {
            var principalAccount = await _savingsAccountRepository.GetPrincipalAccountAsync(account.ApplicationUserId)
                ?? throw new DomainException("Client does not have an active principal account to receive the balance.");

            await _transactionService.TransferAsync(
                account.Id, principalAccount.Id, account.Balance,
                "Balance transfer on secondary account cancellation");
        }

        account.Status = AccountStatus.Cancelled;
        _savingsAccountRepository.Update(account);
        await _savingsAccountRepository.SaveChangesAsync();
    }

    public async Task<PagedResult<SavingsAccountDto>> GetAccountsAsync(
        string? status, string? type, string? cedula, int pageNumber, int pageSize)
    {
        AccountStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<AccountStatus>(status, true, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        AccountType? typeEnum = null;
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<AccountType>(type, true, out var parsedType))
        {
            typeEnum = parsedType;
        }

        var (items, totalCount) = await _savingsAccountRepository.GetPagedAsync(statusEnum, typeEnum, cedula, pageNumber, pageSize);

        return new PagedResult<SavingsAccountDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<SavingsAccountDto?> GetAccountByNumberAsync(string accountNumber)
    {
        var account = await _savingsAccountRepository.GetByAccountNumberWithUserAsync(accountNumber);

        return account is null ? null : MapToDto(account);
    }

    public async Task<PagedResult<TransactionDto>> GetAccountTransactionsAsync(
        string accountNumber, int pageNumber, int pageSize)
    {
        var account = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber)
            ?? throw new NotFoundException(nameof(SavingsAccount), accountNumber);

        var (transactions, totalCount) = await _transactionRepository.GetPagedByAccountIdAsync(account.Id, pageNumber, pageSize);

        return new PagedResult<TransactionDto>
        {
            Items = transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<SavingsAccountDto?> GetPrincipalAccountAsync(string applicationUserId)
    {
        var account = await _savingsAccountRepository.GetPrincipalAccountAsync(applicationUserId);

        return account is null ? null : MapToDto(account);
    }

    private static SavingsAccountDto MapToDto(SavingsAccount account) => new()
    {
        Id = account.Id,
        AccountNumber = account.AccountNumber,
        ApplicationUserId = account.ApplicationUserId,
        OwnerFullName = account.ApplicationUser is not null
            ? $"{account.ApplicationUser.FirstName} {account.ApplicationUser.LastName}"
            : string.Empty,
        OwnerCedula = account.ApplicationUser?.Cedula ?? string.Empty,
        Type = account.Type.ToString(),
        Status = account.Status.ToString(),
        Balance = account.Balance
    };

    private async Task<string> GenerateUniqueAccountNumberAsync()
    {
        string accountNumber;

        do
        {
            accountNumber = Random.Shared.NextInt64(100_000_000, 999_999_999).ToString();
        }
        while (await _savingsAccountRepository.AccountNumberExistsAsync(accountNumber));

        return accountNumber;
    }

    public async Task<IReadOnlyList<SavingsAccountDto>> GetAccountsByUserIdAsync(string applicationUserId)
    {
        var accounts = await _savingsAccountRepository.Query()
            .Include(a => a.ApplicationUser)
            .Where(a => a.ApplicationUserId == applicationUserId)
            .ToListAsync();

        return accounts.Select(MapToDto).ToList();
    }
}