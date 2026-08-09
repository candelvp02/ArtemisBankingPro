using ArtemisBankingPro.Domain.Entities;

namespace ArtemisBankingPro.Domain.Interfaces;

public interface IBeneficiaryRepository : IGenericRepository<Beneficiary>
{
    Task<IReadOnlyList<Beneficiary>> GetByUserIdAsync(string userId);
    Task<bool> ExistsAsync(string userId, int savingsAccountId);
}