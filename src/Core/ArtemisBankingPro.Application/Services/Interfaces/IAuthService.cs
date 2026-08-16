using ArtemisBankingPro.Application.DTOs.Account;

namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(string userName, string password);
    Task ConfirmAccountAsync(string userId, string token);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(string email, string token, string newPassword);
}