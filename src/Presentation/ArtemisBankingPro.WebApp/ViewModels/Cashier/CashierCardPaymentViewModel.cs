using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Cashier;

public class CashierCardPaymentViewModel
{
    [Required]
    [Display(Name = "From account number")]
    public string FromAccountNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Card number")]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}