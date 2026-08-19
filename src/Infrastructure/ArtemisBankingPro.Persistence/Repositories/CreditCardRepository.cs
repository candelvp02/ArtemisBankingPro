using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Persistence.Repositories;

public class CreditCardRepository : GenericRepository<CreditCard>, ICreditCardRepository
{
    public CreditCardRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CreditCard?> GetByCardNumberAsync(string cardNumber) =>
        await DbSet.FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

    public async Task<CreditCard?> GetWithConsumptionsAsync(int creditCardId) =>
        await DbSet
            .Include(c => c.Consumptions)
            .Include(c => c.ApplicationUser)
            .FirstOrDefaultAsync(c => c.Id == creditCardId);

    public async Task<bool> CardNumberExistsAsync(string cardNumber) =>
        await DbSet.AnyAsync(c => c.CardNumber == cardNumber);
    public async Task<(IReadOnlyList<CreditCard> Items, int TotalCount)> GetPagedAsync(CreditCardStatus? status, string? cedula, int pageNumber, int pageSize)
    {
        var query = DbSet
            .Include(c => c.ApplicationUser)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(cedula))
        {
            query = query.Where(c => c.ApplicationUser!.Cedula.Contains(cedula));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.CardNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<CreditCard?> GetByIdWithUserAsync(int cardId)
    {
        return await DbSet
            .Include(c => c.ApplicationUser)
            .FirstOrDefaultAsync(c => c.Id == cardId);
    }
}