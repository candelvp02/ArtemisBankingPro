namespace ArtemisBankingPro.Application.DTOs.Beneficiaries;

public class BeneficiaryDto
{
    public int Id { get; set; }
    public string Alias { get; set; } = string.Empty;
    public int SavingsAccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
}