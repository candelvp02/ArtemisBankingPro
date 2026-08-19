using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.CreditCards;

namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface ICreditCardService
{
    Task<int> AssignCardAsync(AssignCreditCardDto dto);
    Task UpdateLimitAsync(int cardId, decimal newLimit);
    Task CancelCardAsync(int cardId);
    Task<PagedResult<CreditCardDto>> GetCardsAsync(string? status, string? cedula, int pageNumber, int pageSize);
    Task<CreditCardDto?> GetCardByIdAsync(int cardId);
    Task<IReadOnlyList<CardConsumptionDto>> GetCardConsumptionsAsync(int cardId);
}