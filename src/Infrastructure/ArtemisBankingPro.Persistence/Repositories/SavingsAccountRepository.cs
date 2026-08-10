using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Persistence.Repositories;

public class SavingsAccountRepository : GenericRepository<SavingsAccount>, ISavingsAccountRepository
{
    public SavingsAccountRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber) =>
        await DbSet.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

    public async Task<SavingsAccount?> GetPrincipalAccountAsync(string userId) =>
        await DbSet.FirstOrDefaultAsync(a =>
            a.ApplicationUserId == userId &&
            a.Type == AccountType.Principal &&
            a.Status == AccountStatus.Active);

    public async Task<bool> AccountNumberExistsAsync(string accountNumber) =>
        await DbSet.AnyAsync(a => a.AccountNumber == accountNumber);
}