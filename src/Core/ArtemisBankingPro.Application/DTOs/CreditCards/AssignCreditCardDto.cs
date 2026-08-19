namespace ArtemisBankingPro.Application.DTOs.CreditCards;

public class AssignCreditCardDto
{
    public string ApplicationUserId { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
}