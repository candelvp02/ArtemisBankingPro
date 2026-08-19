namespace ArtemisBankingPro.Application.DTOs.CreditCards;

public class CardConsumptionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}