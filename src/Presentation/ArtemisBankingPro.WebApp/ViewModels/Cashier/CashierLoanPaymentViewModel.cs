using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Cashier;

public class CashierLoanPaymentViewModel
{
    [Required]
    [Display(Name = "From account number")]
    public string FromAccountNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Loan number")]
    public string LoanNumber { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}