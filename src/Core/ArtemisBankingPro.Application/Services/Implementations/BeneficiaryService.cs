using ArtemisBankingPro.Application.DTOs.Beneficiaries;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.Domain.Interfaces;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class BeneficiaryService : IBeneficiaryService
{
    private readonly IBeneficiaryRepository _beneficiaryRepository;
    private readonly ISavingsAccountRepository _savingsAccountRepository;

    public BeneficiaryService(
        IBeneficiaryRepository beneficiaryRepository,
        ISavingsAccountRepository savingsAccountRepository)
    {
        _beneficiaryRepository = beneficiaryRepository;
        _savingsAccountRepository = savingsAccountRepository;
    }

    public async Task<int> AddBeneficiaryAsync(string applicationUserId, string accountNumber, string alias)
    {
        var account = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber)
            ?? throw new DomainException("Account not found.");

        if (account.Status != AccountStatus.Active)
        {
            throw new DomainException("Account is not active.");
        }

        if (account.ApplicationUserId == applicationUserId)
        {
            throw new DomainException("You cannot add your own account as a beneficiary.");
        }

        if (await _beneficiaryRepository.ExistsAsync(applicationUserId, account.Id))
        {
            throw new DomainException("This account is already registered as a beneficiary.");
        }

        var beneficiary = new Beneficiary
        {
            ApplicationUserId = applicationUserId,
            SavingsAccountId = account.Id,
            Alias = alias
        };

        await _beneficiaryRepository.AddAsync(beneficiary);
        await _beneficiaryRepository.SaveChangesAsync();

        return beneficiary.Id;
    }

    public async Task RemoveBeneficiaryAsync(int beneficiaryId, string applicationUserId)
    {
        var beneficiary = await _beneficiaryRepository.GetByIdAsync(beneficiaryId)
            ?? throw new NotFoundException(nameof(Beneficiary), beneficiaryId);

        if (beneficiary.ApplicationUserId != applicationUserId)
        {
            throw new UnauthorizedAccessException("You cannot remove another client's beneficiary.");
        }

        _beneficiaryRepository.Remove(beneficiary);
        await _beneficiaryRepository.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<BeneficiaryDto>> GetBeneficiariesAsync(string applicationUserId)
    {
        var beneficiaries = await _beneficiaryRepository.GetByUserIdAsync(applicationUserId);

        return beneficiaries.Select(b => new BeneficiaryDto
        {
            Id = b.Id,
            Alias = b.Alias,
            SavingsAccountId = b.SavingsAccountId,
            AccountNumber = b.SavingsAccount.AccountNumber,
            OwnerFullName = string.Empty
        }).ToList();
    }
}