using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Client;

public class LoanPaymentViewModel
{
    [Required]
    [Display(Name = "From account")]
    public string FromAccountNumber { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public List<string> AvailableAccounts { get; set; } = [];
    public string? LoanNumber { get; set; }
}