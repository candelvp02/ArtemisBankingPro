using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Commerce;

namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface ICommerceService
{
    Task<int> CreateCommerceAsync(CreateCommerceDto dto);
    Task UpdateCommerceAsync(UpdateCommerceDto dto);
    Task ChangeStatusAsync(int commerceId, bool isActive);
    Task<PagedResult<CommerceDto>> GetCommercesAsync(int pageNumber, int pageSize);
    Task<CommerceDto?> GetCommerceByIdAsync(int commerceId);
    Task<CommerceDto?> GetCommerceByUserIdAsync(string applicationUserId);
}