namespace ArtemisBankingPro.Application.DTOs.Loans;

public class AssignLoanDto
{
    public string ApplicationUserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public int TermMonths { get; set; }
}