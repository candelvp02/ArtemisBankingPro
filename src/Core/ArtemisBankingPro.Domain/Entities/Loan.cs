using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Entities;

public class Loan : BaseEntity
{
    public string LoanNumber { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    public ICollection<LoanInstallment> Installments { get; set; } = new List<LoanInstallment>();
}