namespace ArtemisBankingPro.WebApp.ViewModels.Client;

public class AccountSummaryViewModel
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class CardSummaryViewModel
{
    public int Id { get; set; }
    public string MaskedNumber { get; set; } = string.Empty;
    public decimal AvailableCredit { get; set; }
}

public class LoanSummaryViewModel
{
    public string LoanNumber { get; set; } = string.Empty;
    public decimal MonthlyPayment { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ClientHomeViewModel
{
    public List<AccountSummaryViewModel> Accounts { get; set; } = [];
    public List<CardSummaryViewModel> Cards { get; set; } = [];
    public LoanSummaryViewModel? Loan { get; set; }
}