using ArtemisBankingPro.Domain.Entities;

namespace ArtemisBankingPro.Domain.Interfaces;

public interface ITransactionRepository : IGenericRepository<Transaction>
{
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedByAccountIdAsync(int accountId, int pageNumber, int pageSize);
}