namespace ArtemisBankingPro.WebApp.ViewModels.CreditCards;

public class CreditCardListItemViewModel
{
    public int Id { get; set; }
    public string MaskedNumber { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerCedula { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreditCardListViewModel
{
    public List<CreditCardListItemViewModel> Cards { get; set; } = [];
    public string? StatusFilter { get; set; }
    public string? CedulaFilter { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
}