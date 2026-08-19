using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.SavingsAccounts;

public class CreateSecondaryAccountViewModel
{
    [Required]
    [Display(Name = "Client cedula")]
    public string Cedula { get; set; } = string.Empty;

    [Display(Name = "Initial amount")]
    [Range(0, double.MaxValue)]
    public decimal InitialAmount { get; set; }
}