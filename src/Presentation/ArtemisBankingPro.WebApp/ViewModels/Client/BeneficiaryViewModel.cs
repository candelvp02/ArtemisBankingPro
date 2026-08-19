using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Client;

public class BeneficiaryListItemViewModel
{
    public int Id { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}

public class BeneficiariesViewModel
{
    public List<BeneficiaryListItemViewModel> Beneficiaries { get; set; } = [];
    public AddBeneficiaryViewModel NewBeneficiary { get; set; } = new();
}

public class AddBeneficiaryViewModel
{
    [Required]
    [Display(Name = "Account number")]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    public string Alias { get; set; } = string.Empty;
}