namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface ISavingsAccountService
{
    Task<int> CreatePrincipalAccountAsync(string applicationUserId, decimal initialAmount);
}