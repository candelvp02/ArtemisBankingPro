using ArtemisBankingPro.Application.Common.Email;
using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Application.Services.Implementations;

public class HermesPayService : IHermesPayService
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICommerceRepository _commerceRepository;
    private readonly IGenericRepository<CardConsumption> _consumptionRepository;
    private readonly IPasswordHasher _hasher;
    private readonly ITransactionService _transactionService;
    private readonly IEmailService _emailService;

    public HermesPayService(
        ICreditCardRepository creditCardRepository,
        ICommerceRepository commerceRepository,
        IGenericRepository<CardConsumption> consumptionRepository,
        IPasswordHasher hasher,
        ITransactionService transactionService,
        IEmailService emailService)
    {
        _creditCardRepository = creditCardRepository;
        _commerceRepository = commerceRepository;
        _consumptionRepository = consumptionRepository;
        _hasher = hasher;
        _transactionService = transactionService;
        _emailService = emailService;
    }

    public async Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentDto dto)
    {
        var commerce = await _commerceRepository.Query()
            .Include(c => c.SavingsAccount)
            .Include(c => c.ApplicationUser)
            .FirstOrDefaultAsync(c => c.Id == dto.CommerceId)
            ?? throw new NotFoundException(nameof(Commerce), dto.CommerceId);

        var card = await _creditCardRepository.Query()
            .Include(c => c.ApplicationUser)
            .FirstOrDefaultAsync(c => c.CardNumber == dto.CardNumber);

        if (card is null || card.ExpirationDate < DateTime.UtcNow ||
            card.Status != CreditCardStatus.Active || !_hasher.Verify(dto.Cvc, card.CvcHash))
        {
            return await RejectAsync(null, dto.CommerceId, dto.Amount, "Invalid card data.");
        }

        if (card.AvailableCredit < dto.Amount)
        {
            return await RejectAsync(card.Id, dto.CommerceId, dto.Amount, "Insufficient available credit.");
        }

        card.CurrentDebt += dto.Amount;
        _creditCardRepository.Update(card);
        await _creditCardRepository.SaveChangesAsync();

        var consumption = new CardConsumption
        {
            CreditCardId = card.Id,
            CommerceId = dto.CommerceId,
            Amount = dto.Amount,
            Status = ConsumptionStatus.Approved
        };

        await _consumptionRepository.AddAsync(consumption);
        await _consumptionRepository.SaveChangesAsync();

        await _transactionService.RegisterTransactionAsync(
            commerce.SavingsAccountId, TransactionType.Credit, dto.Amount,
            $"Hermes Pay payment from card ending {card.CardNumber[^4..]}");

        await _emailService.SendAsync(
            card.ApplicationUser.Email!, "Payment Approved",
            EmailTemplates.TransactionNotification(card.ApplicationUser.FirstName, $"Payment to {commerce.Name}", dto.Amount));

        await _emailService.SendAsync(
            commerce.Email, "Payment Received",
            EmailTemplates.TransactionNotification(commerce.Name, "Payment received via Hermes Pay", dto.Amount));

        return new PaymentResultDto { Approved = true, RemainingCredit = card.AvailableCredit };
    }

    public async Task<PagedResult<CommerceTransactionDto>> GetCommerceTransactionsAsync(
        int commerceId, int pageNumber, int pageSize)
    {
        var query = _consumptionRepository.Query()
            .Where(c => c.CommerceId == commerceId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();

        var consumptions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<CommerceTransactionDto>
        {
            Items = consumptions.Select(c => new CommerceTransactionDto
            {
                Amount = c.Amount,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private async Task<PaymentResultDto> RejectAsync(
        int? creditCardId, int commerceId, decimal amount, string reason)
    {
        if (creditCardId is not null)
        {
            var consumption = new CardConsumption
            {
                CreditCardId = creditCardId.Value,
                CommerceId = commerceId,
                Amount = amount,
                Status = ConsumptionStatus.Rejected,
                RejectionReason = reason
            };

            await _consumptionRepository.AddAsync(consumption);
            await _consumptionRepository.SaveChangesAsync();
        }

        return new PaymentResultDto { Approved = false, RejectionReason = reason };
    }
}