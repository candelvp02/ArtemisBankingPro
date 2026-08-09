using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Entities;

public class CreditCard : BaseEntity
{
    public string CardNumber { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public DateTime ExpirationDate { get; set; }
    public string CvcHash { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public CreditCardStatus Status { get; set; } = CreditCardStatus.Active;

    public decimal AvailableCredit => CreditLimit - CurrentDebt;
    public string MaskedNumber => CardNumber.Length >= 4
        ? $"**** **** **** {CardNumber[^4..]}"
        : CardNumber;

    public ICollection<CardConsumption> Consumptions { get; set; } = new List<CardConsumption>();
}