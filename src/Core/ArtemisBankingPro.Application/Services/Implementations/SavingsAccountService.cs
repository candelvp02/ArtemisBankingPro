using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class SavingsAccountService : ISavingsAccountService
{
    private readonly ISavingsAccountRepository _savingsAccountRepository;
    private readonly IGenericRepository<Transaction> _transactionRepository;

    public SavingsAccountService(
        ISavingsAccountRepository savingsAccountRepository,
        IGenericRepository<Transaction> transactionRepository)
    {
        _savingsAccountRepository = savingsAccountRepository;
        _transactionRepository = transactionRepository;
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
}