namespace ArtemisBankingPro.WebApp.ViewModels.CreditCards;

public class ConsumptionItemViewModel
{
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CardDetailViewModel
{
    public int Id { get; set; }
    public string MaskedNumber { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public decimal AvailableCredit { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ConsumptionItemViewModel> Consumptions { get; set; } = [];
}

public class EditLimitViewModel
{
    public int Id { get; set; }
    public decimal CreditLimit { get; set; }
}