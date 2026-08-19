namespace ArtemisBankingPro.WebApp.ViewModels.Loans;

public class InstallmentItemViewModel
{
    public int Number { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class LoanDetailViewModel
{
    public int Id { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<InstallmentItemViewModel> Installments { get; set; } = [];
}

public class EditRateViewModel
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.Range(0.01, 100)]
    public decimal NewAnnualRate { get; set; }
}