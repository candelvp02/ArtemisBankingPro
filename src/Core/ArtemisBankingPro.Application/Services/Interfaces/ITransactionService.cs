using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<int> RegisterTransactionAsync(
        int savingsAccountId, TransactionType type, decimal amount, string description);

    Task TransferAsync(
        int fromAccountId, int toAccountId, decimal amount, string description);
}