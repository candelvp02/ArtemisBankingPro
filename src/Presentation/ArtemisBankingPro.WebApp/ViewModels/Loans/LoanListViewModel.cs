namespace ArtemisBankingPro.WebApp.ViewModels.Loans;

public class LoanListItemViewModel
{
    public int Id { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerCedula { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal MonthlyPayment { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class LoanListViewModel
{
    public List<LoanListItemViewModel> Loans { get; set; } = [];
    public string? StatusFilter { get; set; }
    public string? CedulaFilter { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
}