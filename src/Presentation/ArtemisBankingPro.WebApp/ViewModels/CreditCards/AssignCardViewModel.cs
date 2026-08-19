using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.CreditCards;

public class AssignCardViewModel
{
    [Required]
    [Display(Name = "Client cedula")]
    public string Cedula { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Credit limit")]
    [Range(1, double.MaxValue)]
    public decimal CreditLimit { get; set; }
}