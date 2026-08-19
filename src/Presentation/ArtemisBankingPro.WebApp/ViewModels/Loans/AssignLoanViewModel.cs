using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Loans;

public class AssignLoanViewModel
{
    [Required]
    [Display(Name = "Client cedula")]
    public string Cedula { get; set; } = string.Empty;

    [Required]
    [Range(1, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Annual interest rate (%)")]
    [Range(0.01, 100)]
    public decimal AnnualInterestRate { get; set; }

    [Required]
    [Display(Name = "Term (months)")]
    [Range(1, 360)]
    public int TermMonths { get; set; }
}