using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Commerce;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class CommerceService : ICommerceService
{
    private readonly ICommerceRepository _commerceRepository;
    private readonly IIdentityService _identityService;
    private readonly ISavingsAccountService _savingsAccountService;
    private readonly ISavingsAccountRepository _savingsAccountRepository;

    public CommerceService(
        ICommerceRepository commerceRepository,
        IIdentityService identityService,
        ISavingsAccountService savingsAccountService,
        ISavingsAccountRepository savingsAccountRepository)
    {
        _commerceRepository = commerceRepository;
        _identityService = identityService;
        _savingsAccountService = savingsAccountService;
        _savingsAccountRepository = savingsAccountRepository;
    }

    public async Task<int> CreateCommerceAsync(CreateCommerceDto dto)
    {
        if (await _commerceRepository.GetByRncAsync(dto.Rnc) is not null)
        {
            throw new DomainException("RNC is already registered.");
        }

        if (await _commerceRepository.GetByEmailAsync(dto.Email) is not null)
        {
            throw new DomainException("Email is already registered.");
        }

        var (result, userId) = await _identityService.CreateUserAsync(
            dto.UserName, dto.Email, dto.Password, UserRole.Commerce.ToString(),
            dto.Name, "Commerce", dto.Rnc);

        if (!result.Succeeded)
        {
            throw new DomainException(string.Join(" | ", result.Errors));
        }

        var accountId = await _savingsAccountService.CreatePrincipalAccountAsync(userId, 0);
        var account = await _savingsAccountRepository.GetByIdAsync(accountId)
            ?? throw new DomainException("Failed to create commerce principal account.");

        var commerce = new Commerce
        {
            Name = dto.Name,
            Rnc = dto.Rnc,
            Email = dto.Email,
            ApplicationUserId = userId,
            SavingsAccountId = account.Id,
            Status = CommerceStatus.Active
        };

        await _commerceRepository.AddAsync(commerce);
        await _commerceRepository.SaveChangesAsync();

        return commerce.Id;
    }

    public async Task UpdateCommerceAsync(UpdateCommerceDto dto)
    {
        var commerce = await _commerceRepository.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException(nameof(Commerce), dto.Id);

        commerce.Name = dto.Name;
        commerce.Email = dto.Email;

        _commerceRepository.Update(commerce);
        await _commerceRepository.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(int commerceId, bool isActive)
    {
        var commerce = await _commerceRepository.GetByIdAsync(commerceId)
            ?? throw new NotFoundException(nameof(Commerce), commerceId);

        commerce.Status = isActive ? CommerceStatus.Active : CommerceStatus.Inactive;
        _commerceRepository.Update(commerce);
        await _commerceRepository.SaveChangesAsync();

        if (!isActive)
        {
            await _identityService.SetUserActiveStatusAsync(commerce.ApplicationUserId, false);
        }
    }

    public async Task<PagedResult<CommerceDto>> GetCommercesAsync(int pageNumber, int pageSize)
    {
        var query = _commerceRepository.Query()
            .Include(c => c.SavingsAccount)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var commerces = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<CommerceDto>
        {
            Items = commerces.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CommerceDto?> GetCommerceByIdAsync(int commerceId)
    {
        var commerce = await _commerceRepository.Query()
            .Include(c => c.SavingsAccount)
            .FirstOrDefaultAsync(c => c.Id == commerceId);

        return commerce is null ? null : MapToDto(commerce);
    }

    public async Task<CommerceDto?> GetCommerceByUserIdAsync(string applicationUserId)
    {
        var commerce = await _commerceRepository.GetByApplicationUserIdAsync(applicationUserId);

        return commerce is null ? null : MapToDto(commerce);
    }

    private static CommerceDto MapToDto(Commerce commerce) => new()
    {
        Id = commerce.Id,
        Name = commerce.Name,
        Rnc = commerce.Rnc,
        Email = commerce.Email,
        ApplicationUserId = commerce.ApplicationUserId,
        SavingsAccountNumber = commerce.SavingsAccount?.AccountNumber ?? string.Empty,
        Status = commerce.Status.ToString()
    };
}