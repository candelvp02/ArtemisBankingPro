using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(IdentityResultDto Result, string UserId)> CreateUserAsync(
        string userName, string email, string password, string role,
        string firstName, string lastName, string cedula)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = false,
            FirstName = firstName,
            LastName = lastName,
            Cedula = cedula,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.MaxValue
        };

        var createResult = await _userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            return (MapResult(createResult), string.Empty);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);

        return (MapResult(roleResult), user.Id);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityResultDto> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
        }

        return MapResult(result);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException("User not found.");

        user.LockoutEnd = DateTimeOffset.MaxValue;
        await _userManager.UpdateAsync(user);

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResultDto> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException("User not found.");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
        }

        return MapResult(result);
    }

    public async Task<bool> CheckPasswordAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);

        return user is not null && await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> IsUserActiveAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user is null)
        {
            return false;
        }

        return user.EmailConfirmed && !await _userManager.IsLockedOutAsync(user);
    }

    public async Task<string?> GetUserIdAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);

        return user?.Id;
    }

    public async Task<string?> GetUserRoleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return roles.FirstOrDefault();
    }

    public async Task<IdentityResultDto> SetUserActiveStatusAsync(string userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.LockoutEnd = isActive ? null : DateTimeOffset.MaxValue;

        var result = await _userManager.UpdateAsync(user);

        return MapResult(result);
    }

    public async Task<bool> UserNameExistsAsync(string userName) =>
        await _userManager.FindByNameAsync(userName) is not null;

    public async Task<bool> EmailExistsAsync(string email) =>
        await _userManager.FindByEmailAsync(email) is not null;

    public async Task<bool> CedulaExistsAsync(string cedula) =>
        await _userManager.Users.AnyAsync(u => u.Cedula == cedula);

    public async Task<(IReadOnlyList<ApplicationUser> Users, int TotalCount)> GetUsersPagedAsync(
        string? role, bool excludeCommerceWhenNoRole, int pageNumber, int pageSize)
    {
        IQueryable<ApplicationUser> query = _userManager.Users;

        if (!string.IsNullOrEmpty(role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var ids = usersInRole.Select(u => u.Id).ToList();
            query = query.Where(u => ids.Contains(u.Id));
        }
        else if (excludeCommerceWhenNoRole)
        {
            var commerceUsers = await _userManager.GetUsersInRoleAsync(UserRole.Commerce.ToString());
            var commerceIds = commerceUsers.Select(u => u.Id).ToHashSet();
            query = query.Where(u => !commerceIds.Contains(u.Id));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId) =>
        await _userManager.FindByIdAsync(userId);

    public async Task<IdentityResultDto> UpdateUserProfileAsync(
        string userId, string firstName, string lastName, string email)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.FirstName = firstName;
        user.LastName = lastName;

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, email);

            if (!setEmailResult.Succeeded)
            {
                return MapResult(setEmailResult);
            }
        }

        var result = await _userManager.UpdateAsync(user);

        return MapResult(result);
    }

    private static IdentityResultDto MapResult(IdentityResult result) =>
        result.Succeeded
            ? IdentityResultDto.Success()
            : IdentityResultDto.Failure(result.Errors.Select(e => e.Description));
}