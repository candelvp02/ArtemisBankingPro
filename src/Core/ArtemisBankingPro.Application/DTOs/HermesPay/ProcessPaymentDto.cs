namespace ArtemisBankingPro.Application.DTOs.HermesPay;

public class ProcessPaymentDto
{
    public int CommerceId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public string Cvc { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PaymentResultDto
{
    public bool Approved { get; set; }
    public string? RejectionReason { get; set; }
    public decimal? RemainingCredit { get; set; }
}

public class CommerceTransactionDto
{
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}