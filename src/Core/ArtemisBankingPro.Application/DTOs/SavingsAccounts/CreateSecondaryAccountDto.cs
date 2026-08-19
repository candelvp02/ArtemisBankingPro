namespace ArtemisBankingPro.Application.DTOs.SavingsAccounts;

public class CreateSecondaryAccountDto
{
    public string ApplicationUserId { get; set; } = string.Empty;
    public decimal InitialAmount { get; set; }
}