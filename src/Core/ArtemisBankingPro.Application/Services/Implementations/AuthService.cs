using System.Web;
using ArtemisBankingPro.Application.Common.Email;
using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly AppSettings _appSettings;

    public AuthService(
        IIdentityService identityService,
        IJwtService jwtService,
        IEmailService emailService,
        IOptions<AppSettings> appSettings)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _emailService = emailService;
        _appSettings = appSettings.Value;
    }

    public async Task<LoginResponseDto> LoginAsync(string userName, string password)
    {
        var isActive = await _identityService.IsUserActiveAsync(userName);

        if (!isActive)
        {
            throw new UnauthorizedAccessException("Account is not active or does not exist.");
        }

        var isPasswordValid = await _identityService.CheckPasswordAsync(userName, password);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var userId = await _identityService.GetUserIdAsync(userName)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var role = await _identityService.GetUserRoleAsync(userId)
            ?? throw new DomainException("User does not have an assigned role.");

        var token = _jwtService.GenerateToken(userId, userName, role);

        return new LoginResponseDto
        {
            Token = token,
            UserId = userId,
            UserName = userName,
            Role = role,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task ConfirmAccountAsync(string userId, string token)
    {
        var result = await _identityService.ConfirmEmailAsync(userId, token);

        if (!result.Succeeded)
        {
            throw new DomainException(string.Join(" | ", result.Errors));
        }
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var token = await _identityService.GeneratePasswordResetTokenAsync(email);
        var encodedToken = HttpUtility.UrlEncode(token);
        var encodedEmail = HttpUtility.UrlEncode(email);

        var resetLink = $"{_appSettings.WebAppBaseUrl}/Account/ResetPassword?email={encodedEmail}&token={encodedToken}";

        await _emailService.SendAsync(
            email,
            "Password Reset Request",
            EmailTemplates.PasswordReset(email, resetLink));
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        var result = await _identityService.ResetPasswordAsync(email, token, newPassword);

        if (!result.Succeeded)
        {
            throw new DomainException(string.Join(" | ", result.Errors));
        }
    }
}