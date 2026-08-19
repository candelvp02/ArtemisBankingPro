using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.SavingsAccounts;

namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface ISavingsAccountService
{
    Task<int> CreatePrincipalAccountAsync(string applicationUserId, decimal initialAmount);
    Task<int> CreateSecondaryAccountAsync(CreateSecondaryAccountDto dto);
    Task CancelAccountAsync(int accountId);
    Task<PagedResult<SavingsAccountDto>> GetAccountsAsync(
        string? status, string? type, string? cedula, int pageNumber, int pageSize);
    Task<SavingsAccountDto?> GetAccountByNumberAsync(string accountNumber);
    Task<PagedResult<TransactionDto>> GetAccountTransactionsAsync(
        string accountNumber, int pageNumber, int pageSize);
    Task<SavingsAccountDto?> GetPrincipalAccountAsync(string applicationUserId);
    Task<IReadOnlyList<SavingsAccountDto>> GetAccountsByUserIdAsync(string applicationUserId);
}