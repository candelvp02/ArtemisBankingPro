using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.HermesPay;

namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface IHermesPayService
{
    Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentDto dto);
    Task<PagedResult<CommerceTransactionDto>> GetCommerceTransactionsAsync(
        int commerceId, int pageNumber, int pageSize);
}