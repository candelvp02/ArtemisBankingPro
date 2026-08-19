namespace ArtemisBankingPro.Application.DTOs.Commerce;

public class CommerceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Rnc { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public string SavingsAccountNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}