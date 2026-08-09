using ArtemisBankingPro.Domain.Common;

namespace ArtemisBankingPro.Domain.Entities;

public class Beneficiary : BaseEntity
{
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public int SavingsAccountId { get; set; }
    public SavingsAccount SavingsAccount { get; set; } = null!;
    public string Alias { get; set; } = string.Empty;
}