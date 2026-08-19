using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class TransactionService : ITransactionService
{
    private readonly ISavingsAccountRepository _accountRepository;
    private readonly IGenericRepository<Transaction> _transactionRepository;

    public TransactionService(
        ISavingsAccountRepository accountRepository,
        IGenericRepository<Transaction> transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<int> RegisterTransactionAsync(
        int savingsAccountId, TransactionType type, decimal amount, string description,
        string? performedByUserId = null)
    {
        var account = await _accountRepository.GetByIdAsync(savingsAccountId)
            ?? throw new NotFoundException(nameof(SavingsAccount), savingsAccountId);

        if (account.Status != AccountStatus.Active)
        {
            throw new DomainException("Cannot operate on a cancelled account.");
        }

        if (type == TransactionType.Debit && account.Balance < amount)
        {
            throw new DomainException("Insufficient funds.");
        }

        account.Balance = type == TransactionType.Credit
            ? account.Balance + amount
            : account.Balance - amount;

        _accountRepository.Update(account);

        var transaction = new Transaction
        {
            SavingsAccountId = savingsAccountId,
            Type = type,
            Amount = amount,
            BalanceAfter = account.Balance,
            Description = description,
            PerformedByUserId = performedByUserId
        };

        await _transactionRepository.AddAsync(transaction);
        await _accountRepository.SaveChangesAsync();

        return transaction.Id;
    }

    public async Task TransferAsync(
        int fromAccountId, int toAccountId, decimal amount, string description,
        string? performedByUserId = null)
    {
        await RegisterTransactionAsync(
            fromAccountId, TransactionType.Debit, amount, $"Transfer out - {description}", performedByUserId);

        await RegisterTransactionAsync(
            toAccountId, TransactionType.Credit, amount, $"Transfer in - {description}", performedByUserId);
    }

    public async Task<int> CountTodayByPerformedUserAsync(string performedByUserId)
    {
        var today = DateTime.UtcNow.Date;

        return await _transactionRepository.Query()
            .Where(t => t.PerformedByUserId == performedByUserId && t.CreatedAt.Date == today)
            .CountAsync();
    }
}