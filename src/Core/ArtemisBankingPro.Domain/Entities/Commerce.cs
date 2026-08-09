using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Entities;

public class Commerce : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Rnc { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public int SavingsAccountId { get; set; }
    public SavingsAccount SavingsAccount { get; set; } = null!;
    public CommerceStatus Status { get; set; } = CommerceStatus.Active;

    public ICollection<CardConsumption> Consumptions { get; set; } = new List<CardConsumption>();
}