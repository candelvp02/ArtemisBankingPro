using ArtemisBankingPro.Domain.Entities;
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
}