using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Interfaces;

public interface ICreditCardRepository : IGenericRepository<CreditCard>
{
    Task<CreditCard?> GetByCardNumberAsync(string cardNumber);
    Task<CreditCard?> GetWithConsumptionsAsync(int creditCardId);
    Task<bool> CardNumberExistsAsync(string cardNumber);
    Task<(IReadOnlyList<CreditCard> Items, int TotalCount)> GetPagedAsync(CreditCardStatus? status, string? cedula, int pageNumber, int pageSize);
    Task<CreditCard?> GetByIdWithUserAsync(int cardId);
}