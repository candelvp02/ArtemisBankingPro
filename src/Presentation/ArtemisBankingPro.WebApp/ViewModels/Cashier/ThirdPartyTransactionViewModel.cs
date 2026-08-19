using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Cashier;

public class ThirdPartyTransactionViewModel
{
    [Required]
    [Display(Name = "From account number")]
    public string FromAccountNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "To account number")]
    public string ToAccountNumber { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}