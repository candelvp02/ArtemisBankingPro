using ArtemisBankingPro.Domain.Entities;

namespace ArtemisBankingPro.Application.Common.Interfaces;

public class IdentityResultDto
{
    public bool Succeeded { get; init; }
    public IEnumerable<string> Errors { get; init; } = [];

    public static IdentityResultDto Success() => new() { Succeeded = true };
    public static IdentityResultDto Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors };
}

public interface IIdentityService
{
    Task<(IdentityResultDto Result, string UserId)> CreateUserAsync(
        string userName, string email, string password, string role,
        string firstName, string lastName, string cedula);

    Task<string> GenerateEmailConfirmationTokenAsync(string userId);
    Task<IdentityResultDto> ConfirmEmailAsync(string userId, string token);

    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<IdentityResultDto> ResetPasswordAsync(string email, string token, string newPassword);

    Task<bool> CheckPasswordAsync(string userName, string password);
    Task<bool> IsUserActiveAsync(string userName);
    Task<string?> GetUserIdAsync(string userName);
    Task<string?> GetUserRoleAsync(string userId);
    Task<IdentityResultDto> SetUserActiveStatusAsync(string userId, bool isActive);

    Task<bool> UserNameExistsAsync(string userName);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CedulaExistsAsync(string cedula);

    Task<(IReadOnlyList<ApplicationUser> Users, int TotalCount)> GetUsersPagedAsync(
        string? role, bool excludeCommerceWhenNoRole, int pageNumber, int pageSize);

    Task<ApplicationUser?> GetUserByIdAsync(string userId);

    Task<IdentityResultDto> UpdateUserProfileAsync(
        string userId, string firstName, string lastName, string email);
    Task<ApplicationUser?> GetUserByCedulaAsync(string cedula);
}