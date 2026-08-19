using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.WebApp.Filters;
using ArtemisBankingPro.WebApp.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApp.Controllers;

[RoleAuthorize("Administrator")]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ISavingsAccountService _savingsAccountService;
    private readonly ILoanService _loanService;
    private readonly ICreditCardService _creditCardService;

    public AdminController(
        IUserService userService,
        ISavingsAccountService savingsAccountService,
        ILoanService loanService,
        ICreditCardService creditCardService)
    {
        _userService = userService;
        _savingsAccountService = savingsAccountService;
        _loanService = loanService;
        _creditCardService = creditCardService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var (activeClients, inactiveClients) = await _userService.GetClientCountsAsync();

        var accounts = await _savingsAccountService.GetAccountsAsync(
            status: "Active", type: null, cedula: null, pageNumber: 1, pageSize: int.MaxValue);

        var loans = await _loanService.GetLoansAsync(
            status: "Active", cedula: null, pageNumber: 1, pageSize: int.MaxValue);

        var cards = await _creditCardService.GetCardsAsync(
            status: "Active", cedula: null, pageNumber: 1, pageSize: int.MaxValue);

        var averageDebt = loans.Items.Count > 0 ? loans.Items.Average(l => l.Amount) : 0;

        var viewModel = new AdminDashboardViewModel
        {
            ActiveClients = activeClients,
            InactiveClients = inactiveClients,
            ActiveSavingsAccounts = accounts.TotalCount,
            ActiveLoans = loans.TotalCount,
            ActiveCreditCards = cards.TotalCount,
            AverageDebtPerClient = averageDebt,
            TotalHistoricalTransactions = 0,
            TodayTransactions = 0,
            TotalHistoricalPayments = 0,
            TodayPayments = 0
        };

        return View(viewModel);
    }
}