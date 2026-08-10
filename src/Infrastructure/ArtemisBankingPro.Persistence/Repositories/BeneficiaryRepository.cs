using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Persistence.Repositories;

public class BeneficiaryRepository : GenericRepository<Beneficiary>, IBeneficiaryRepository
{
    public BeneficiaryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Beneficiary>> GetByUserIdAsync(string userId) =>
        await DbSet
            .Include(b => b.SavingsAccount)
            .Where(b => b.ApplicationUserId == userId)
            .ToListAsync();

    public async Task<bool> ExistsAsync(string userId, int savingsAccountId) =>
        await DbSet.AnyAsync(b => b.ApplicationUserId == userId && b.SavingsAccountId == savingsAccountId);
}