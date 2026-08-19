using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Client;

public class SelfTransferViewModel
{
    [Required]
    [Display(Name = "From account")]
    public string FromAccountNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "To account")]
    public string ToAccountNumber { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public List<string> AvailableAccounts { get; set; } = [];
}