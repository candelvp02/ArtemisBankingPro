using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Entities;

public class CardConsumption : BaseEntity
{
    public int CreditCardId { get; set; }
    public CreditCard CreditCard { get; set; } = null!;
    public int? CommerceId { get; set; }
    public Commerce? Commerce { get; set; }
    public decimal Amount { get; set; }
    public ConsumptionStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}