using ArtemisBankingPro.Application.Common.Email;
using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.CreditCards;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class CreditCardService : ICreditCardService
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ISavingsAccountRepository _savingsAccountRepository;
    private readonly IPasswordHasher _hasher;
    private readonly IEmailService _emailService;
    private readonly ITransactionService _transactionService;

    public CreditCardService(
        ICreditCardRepository creditCardRepository,
        ISavingsAccountRepository savingsAccountRepository,
        IPasswordHasher hasher,
        IEmailService emailService,
        ITransactionService transactionService)
    {
        _creditCardRepository = creditCardRepository;
        _savingsAccountRepository = savingsAccountRepository;
        _hasher = hasher;
        _emailService = emailService;
        _transactionService = transactionService;
    }

    public async Task<int> AssignCardAsync(AssignCreditCardDto dto)
    {
        var principalAccount = await _savingsAccountRepository.GetPrincipalAccountAsync(dto.ApplicationUserId)
            ?? throw new DomainException("Client does not have an active principal account.");

        var cardNumber = await GenerateUniqueCardNumberAsync();
        var cvc = Random.Shared.Next(100, 1000).ToString("D3");

        var card = new CreditCard
        {
            CardNumber = cardNumber,
            ApplicationUserId = dto.ApplicationUserId,
            ExpirationDate = DateTime.UtcNow.AddYears(4),
            CvcHash = _hasher.Hash(cvc),
            CreditLimit = dto.CreditLimit,
            CurrentDebt = 0,
            Status = CreditCardStatus.Active
        };

        await _creditCardRepository.AddAsync(card);
        await _creditCardRepository.SaveChangesAsync();

        var owner = await _savingsAccountRepository.GetOwnerByAccountIdAsync(principalAccount.Id);

        if (owner?.Email is not null)
        {
            await _emailService.SendAsync(
                owner.Email,
                "New Credit Card Assigned",
                EmailTemplates.CreditCardAssigned(owner.FirstName, card.MaskedNumber));
        }

        return card.Id;
    }

    public async Task UpdateLimitAsync(int cardId, decimal newLimit)
    {
        var card = await _creditCardRepository.GetByIdAsync(cardId)
            ?? throw new NotFoundException(nameof(CreditCard), cardId);

        if (newLimit < card.CurrentDebt)
        {
            throw new DomainException("New limit cannot be lower than the current debt.");
        }

        card.CreditLimit = newLimit;
        _creditCardRepository.Update(card);
        await _creditCardRepository.SaveChangesAsync();
    }

    public async Task CancelCardAsync(int cardId)
    {
        var card = await _creditCardRepository.GetByIdAsync(cardId)
            ?? throw new NotFoundException(nameof(CreditCard), cardId);

        if (card.CurrentDebt > 0)
        {
            throw new DomainException("Cannot cancel a card with pending debt.");
        }

        card.Status = CreditCardStatus.Cancelled;
        _creditCardRepository.Update(card);
        await _creditCardRepository.SaveChangesAsync();
    }

    public async Task<PagedResult<CreditCardDto>> GetCardsAsync(
        string? status, string? cedula, int pageNumber, int pageSize)
    {
        CreditCardStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<CreditCardStatus>(status, true, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        var (items, totalCount) = await _creditCardRepository.GetPagedAsync(statusEnum, cedula, pageNumber, pageSize);

        return new PagedResult<CreditCardDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CreditCardDto?> GetCardByIdAsync(int cardId)
    {
        var card = await _creditCardRepository.GetByIdWithUserAsync(cardId);

        return card is null ? null : MapToDto(card);
    }

    public async Task<IReadOnlyList<CardConsumptionDto>> GetCardConsumptionsAsync(int cardId)
    {
        var card = await _creditCardRepository.GetWithConsumptionsAsync(cardId)
            ?? throw new NotFoundException(nameof(CreditCard), cardId);

        return card.Consumptions
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CardConsumptionDto
            {
                Id = c.Id,
                Amount = c.Amount,
                Status = c.Status.ToString(),
                RejectionReason = c.RejectionReason,
                CreatedAt = c.CreatedAt
            })
            .ToList();
    }

    private static CreditCardDto MapToDto(CreditCard card) => new()
    {
        Id = card.Id,
        MaskedNumber = card.MaskedNumber,
        ApplicationUserId = card.ApplicationUserId,
        OwnerFullName = card.ApplicationUser is not null
            ? $"{card.ApplicationUser.FirstName} {card.ApplicationUser.LastName}"
            : string.Empty,
        OwnerCedula = card.ApplicationUser?.Cedula ?? string.Empty,
        ExpirationDate = card.ExpirationDate,
        CreditLimit = card.CreditLimit,
        CurrentDebt = card.CurrentDebt,
        AvailableCredit = card.AvailableCredit,
        Status = card.Status.ToString()
    };

    private async Task<string> GenerateUniqueCardNumberAsync()
    {
        string cardNumber;

        do
        {
            cardNumber = string.Concat(Enumerable.Range(0, 16).Select(_ => Random.Shared.Next(0, 10).ToString()));
        }
        while (await _creditCardRepository.CardNumberExistsAsync(cardNumber));

        return cardNumber;
    }
    public async Task<IReadOnlyList<CreditCardDto>> GetCardsByUserIdAsync(string applicationUserId)
    {
        var cards = await _creditCardRepository.Query()
            .Include(c => c.ApplicationUser)
            .Where(c => c.ApplicationUserId == applicationUserId)
            .ToListAsync();

        return cards.Select(MapToDto).ToList();
    }

    public async Task PayCardAsync(int cardId, int savingsAccountId, decimal amount)
    {
        var card = await _creditCardRepository.GetByIdAsync(cardId)
            ?? throw new NotFoundException(nameof(CreditCard), cardId);

        if (amount > card.CurrentDebt)
        {
            throw new DomainException("Payment amount exceeds current debt. Overpayment is not allowed.");
        }

        await _transactionService.RegisterTransactionAsync(
            savingsAccountId, TransactionType.Debit, amount, $"Credit card payment - {card.MaskedNumber}");

        card.CurrentDebt -= amount;
        _creditCardRepository.Update(card);
        await _creditCardRepository.SaveChangesAsync();
    }

    public async Task<decimal> CashAdvanceAsync(int cardId, int savingsAccountId, decimal amount)
    {
        const decimal cashAdvanceInterestRate = 0.0625m;

        var card = await _creditCardRepository.GetByIdAsync(cardId)
            ?? throw new NotFoundException(nameof(CreditCard), cardId);

        var totalWithInterest = Math.Round(amount * (1 + cashAdvanceInterestRate), 2);

        if (totalWithInterest > card.AvailableCredit)
        {
            throw new DomainException("Insufficient available credit for this cash advance.");
        }

        card.CurrentDebt += totalWithInterest;
        _creditCardRepository.Update(card);
        await _creditCardRepository.SaveChangesAsync();

        await _transactionService.RegisterTransactionAsync(
            savingsAccountId, TransactionType.Credit, amount, $"Cash advance from card {card.MaskedNumber}");

        return totalWithInterest;
    }

    public async Task<CreditCardDto?> GetCardByNumberAsync(string cardNumber)
    {
        var card = await _creditCardRepository.Query()
            .Include(c => c.ApplicationUser)
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

        return card is null ? null : MapToDto(card);
    }
}