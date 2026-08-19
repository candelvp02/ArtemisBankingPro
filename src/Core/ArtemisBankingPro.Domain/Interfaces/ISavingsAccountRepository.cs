using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Interfaces;

public interface ISavingsAccountRepository : IGenericRepository<SavingsAccount>
{
    Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber);
    Task<SavingsAccount?> GetPrincipalAccountAsync(string userId);
    Task<bool> AccountNumberExistsAsync(string accountNumber);
    Task<(IReadOnlyList<SavingsAccount> Items, int TotalCount)> GetPagedAsync(AccountStatus? status, AccountType? type, string? cedula, int pageNumber, int pageSize);
    Task<SavingsAccount?> GetByAccountNumberWithUserAsync(string accountNumber);
    Task<ApplicationUser?> GetOwnerByAccountIdAsync(int accountId);
}