namespace ArtemisBankingPro.WebApp.ViewModels.SavingsAccounts;

public class SavingsAccountListItemViewModel
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerCedula { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class SavingsAccountListViewModel
{
    public List<SavingsAccountListItemViewModel> Accounts { get; set; } = [];
    public string? StatusFilter { get; set; }
    public string? TypeFilter { get; set; }
    public string? CedulaFilter { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
}