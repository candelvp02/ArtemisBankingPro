using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Entities;

public class Transaction : BaseEntity
{
    public int SavingsAccountId { get; set; }
    public SavingsAccount SavingsAccount { get; set; } = null!;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? RelatedTransactionId { get; set; }
    public string? PerformedByUserId { get; set; }
}