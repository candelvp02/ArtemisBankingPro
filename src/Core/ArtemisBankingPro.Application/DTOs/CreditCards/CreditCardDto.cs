namespace ArtemisBankingPro.Application.DTOs.CreditCards;

public class CreditCardDto
{
    public int Id { get; set; }
    public string MaskedNumber { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerCedula { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public decimal AvailableCredit { get; set; }
    public string Status { get; set; } = string.Empty;
}