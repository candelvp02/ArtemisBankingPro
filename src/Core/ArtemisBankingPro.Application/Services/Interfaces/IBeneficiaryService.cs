using ArtemisBankingPro.Application.DTOs.Beneficiaries;

namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface IBeneficiaryService
{
    Task<int> AddBeneficiaryAsync(string applicationUserId, string accountNumber, string alias);
    Task RemoveBeneficiaryAsync(int beneficiaryId, string applicationUserId);
    Task<IReadOnlyList<BeneficiaryDto>> GetBeneficiariesAsync(string applicationUserId);
}