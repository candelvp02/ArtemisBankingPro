namespace ArtemisBankingPro.Application.DTOs.SavingsAccounts;

public class SavingsAccountDto
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerCedula { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
}