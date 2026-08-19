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

    public async Task<(IReadOnlyList<SavingsAccount> Items, int TotalCount)> GetPagedAsync(AccountStatus? status, AccountType? type, string? cedula, int pageNumber, int pageSize)
    {
        var query = DbSet
            .Include(a => a.ApplicationUser)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(a => a.Type == type.Value);
        }

        if (!string.IsNullOrEmpty(cedula))
        {
            query = query.Where(a => a.ApplicationUser!.Cedula.Contains(cedula));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.AccountNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<SavingsAccount?> GetByAccountNumberWithUserAsync(string accountNumber)
    {
        return await DbSet
            .Include(a => a.ApplicationUser)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<ApplicationUser?> GetOwnerByAccountIdAsync(int accountId)
    {
        return await DbSet
            .Where(a => a.Id == accountId)
            .Select(a => a.ApplicationUser)
            .FirstOrDefaultAsync();
    }
}