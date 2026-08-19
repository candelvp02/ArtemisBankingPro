namespace ArtemisBankingPro.WebApp.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalHistoricalTransactions { get; set; }
    public int TodayTransactions { get; set; }
    public int TotalHistoricalPayments { get; set; }
    public int TodayPayments { get; set; }
    public int ActiveClients { get; set; }
    public int InactiveClients { get; set; }
    public int ActiveSavingsAccounts { get; set; }
    public int ActiveLoans { get; set; }
    public int ActiveCreditCards { get; set; }
    public decimal AverageDebtPerClient { get; set; }
}