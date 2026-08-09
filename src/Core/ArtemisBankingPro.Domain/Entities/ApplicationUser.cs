using Microsoft.AspNetCore.Identity;

namespace ArtemisBankingPro.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<SavingsAccount> SavingsAccounts { get; set; } = new List<SavingsAccount>();
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public ICollection<CreditCard> CreditCards { get; set; } = new List<CreditCard>();
    public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
}