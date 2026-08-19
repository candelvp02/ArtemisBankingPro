namespace ArtemisBankingPro.WebApp.ViewModels.Client;

public class ConsumptionItemViewModel
{
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CardDetailViewModel
{
    public int Id { get; set; }
    public string MaskedNumber { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public decimal AvailableCredit { get; set; }
    public List<ConsumptionItemViewModel> Consumptions { get; set; } = [];
}