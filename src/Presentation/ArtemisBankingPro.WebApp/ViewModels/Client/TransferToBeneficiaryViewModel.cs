using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Client;

public class TransferToBeneficiaryViewModel
{
    [Required]
    [Display(Name = "From account")]
    public string FromAccountNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Beneficiary")]
    public int BeneficiaryId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public List<string> AvailableAccounts { get; set; } = [];
    public List<BeneficiaryListItemViewModel> Beneficiaries { get; set; } = [];
}