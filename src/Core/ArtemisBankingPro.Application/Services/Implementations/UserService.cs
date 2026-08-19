using System.Web;
using ArtemisBankingPro.Application.Common.Email;
using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Users;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IIdentityService _identityService;
    private readonly ISavingsAccountService _savingsAccountService;
    private readonly IEmailService _emailService;
    private readonly Common.Models.AppSettings _appSettings;

    public UserService(
        IIdentityService identityService,
        ISavingsAccountService savingsAccountService,
        IEmailService emailService,
        IOptions<Common.Models.AppSettings> appSettings)
    {
        _identityService = identityService;
        _savingsAccountService = savingsAccountService;
        _emailService = emailService;
        _appSettings = appSettings.Value;
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(string? role, int pageNumber, int pageSize)
    {
        var (users, totalCount) = await _identityService.GetUsersPagedAsync(
            role, excludeCommerceWhenNoRole: true, pageNumber, pageSize);

        var dtos = new List<UserDto>();

        foreach (var user in users)
        {
            dtos.Add(await MapToDtoAsync(user));
        }

        return new PagedResult<UserDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _identityService.GetUserByIdAsync(userId);

        return user is null ? null : await MapToDtoAsync(user);
    }

    public async Task<string> CreateUserAsync(CreateUserDto dto)
    {
        if (await _identityService.UserNameExistsAsync(dto.UserName))
        {
            throw new DomainException("Username is already taken.");
        }

        if (await _identityService.EmailExistsAsync(dto.Email))
        {
            throw new DomainException("Email is already registered.");
        }

        if (await _identityService.CedulaExistsAsync(dto.Cedula))
        {
            throw new DomainException("Cedula is already registered.");
        }

        var (result, userId) = await _identityService.CreateUserAsync(
            dto.UserName, dto.Email, dto.Password, dto.Role,
            dto.FirstName, dto.LastName, dto.Cedula);

        if (!result.Succeeded)
        {
            throw new DomainException(string.Join(" | ", result.Errors));
        }

        if (dto.Role == UserRole.Client.ToString())
        {
            await _savingsAccountService.CreatePrincipalAccountAsync(userId, dto.InitialAmount ?? 0);
        }

        await SendActivationEmailAsync(userId, dto.UserName, dto.Email);

        return userId;
    }

    public async Task UpdateUserAsync(UpdateUserDto dto)
    {
        var result = await _identityService.UpdateUserProfileAsync(
            dto.Id, dto.FirstName, dto.LastName, dto.Email);

        if (!result.Succeeded)
        {
            throw new DomainException(string.Join(" | ", result.Errors));
        }
    }

    public async Task ChangeUserStatusAsync(string targetUserId, string requestingUserId, bool isActive)
    {
        if (targetUserId == requestingUserId)
        {
            throw new DomainException("You cannot change your own active status.");
        }

        var result = await _identityService.SetUserActiveStatusAsync(targetUserId, isActive);

        if (!result.Succeeded)
        {
            throw new DomainException(string.Join(" | ", result.Errors));
        }
    }

    public Task<bool> UserNameExistsAsync(string userName) => _identityService.UserNameExistsAsync(userName);

    public Task<bool> EmailExistsAsync(string email) => _identityService.EmailExistsAsync(email);

    public Task<bool> CedulaExistsAsync(string cedula) => _identityService.CedulaExistsAsync(cedula);

    private async Task SendActivationEmailAsync(string userId, string userName, string email)
    {
        var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);
        var encodedToken = HttpUtility.UrlEncode(token);
        var activationLink =
            $"{_appSettings.WebAppBaseUrl}/Account/ActivateAccount?userId={userId}&token={encodedToken}";

        await _emailService.SendAsync(
            email,
            "Activate your Artemis Banking Pro account",
            EmailTemplates.AccountActivation(userName, activationLink));
    }

    private async Task<UserDto> MapToDtoAsync(ApplicationUser user)
    {
        var role = await _identityService.GetUserRoleAsync(user.Id) ?? string.Empty;

        var isActive = user.EmailConfirmed &&
            (user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Cedula = user.Cedula,
            Role = role,
            IsActive = isActive
        };
    }
}