using ArtemisBankingPro.Domain.Entities;

namespace ArtemisBankingPro.Domain.Interfaces;

public interface ISavingsAccountRepository : IGenericRepository<SavingsAccount>
{
    Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber);
    Task<SavingsAccount?> GetPrincipalAccountAsync(string userId);
    Task<bool> AccountNumberExistsAsync(string accountNumber);
}