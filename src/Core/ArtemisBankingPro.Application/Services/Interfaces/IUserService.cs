using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Users;

namespace ArtemisBankingPro.Application.Services.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetUsersAsync(string? role, int pageNumber, int pageSize);
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<string> CreateUserAsync(CreateUserDto dto);
    Task UpdateUserAsync(UpdateUserDto dto);
    Task ChangeUserStatusAsync(string targetUserId, string requestingUserId, bool isActive);
    Task<bool> UserNameExistsAsync(string userName);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CedulaExistsAsync(string cedula);
    Task<string?> GetUserIdByCedulaAsync(string cedula);
    Task<(int ActiveCount, int InactiveCount)> GetClientCountsAsync();
}