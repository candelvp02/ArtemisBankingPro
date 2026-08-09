using ArtemisBankingPro.Domain.Entities;

namespace ArtemisBankingPro.Domain.Interfaces;

public interface ICreditCardRepository : IGenericRepository<CreditCard>
{
    Task<CreditCard?> GetByCardNumberAsync(string cardNumber);
    Task<CreditCard?> GetWithConsumptionsAsync(int creditCardId);
    Task<bool> CardNumberExistsAsync(string cardNumber);
}