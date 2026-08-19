using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Cashier;

public class DepositViewModel
{
    [Required]
    [Display(Name = "Account number")]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}