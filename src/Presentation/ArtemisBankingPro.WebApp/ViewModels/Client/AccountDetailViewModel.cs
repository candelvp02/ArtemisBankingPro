namespace ArtemisBankingPro.WebApp.ViewModels.Client;

public class TransactionItemViewModel
{
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AccountDetailViewModel
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public List<TransactionItemViewModel> Transactions { get; set; } = [];
}