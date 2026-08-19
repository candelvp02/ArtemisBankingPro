namespace ArtemisBankingPro.WebApp.ViewModels.Client;

public class InstallmentItemViewModel
{
    public int Number { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class LoanDetailViewModel
{
    public int Id { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal MonthlyPayment { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<InstallmentItemViewModel> Installments { get; set; } = [];
}