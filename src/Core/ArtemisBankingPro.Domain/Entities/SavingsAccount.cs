using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;
using System.Transactions;

namespace ArtemisBankingPro.Domain.Entities;

public class SavingsAccount : BaseEntity
{
    public string AccountNumber { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public AccountType Type { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public decimal Balance { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}