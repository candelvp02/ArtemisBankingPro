using System.Security.Claims;
using ArtemisBankingPro.Application.Common.Email;
using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.WebApp.Filters;
using ArtemisBankingPro.WebApp.ViewModels.Cashier;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApp.Controllers;

[RoleAuthorize("Cashier")]
public class CashierController : Controller
{
    private readonly ISavingsAccountService _savingsAccountService;
    private readonly ICreditCardService _creditCardService;
    private readonly ILoanService _loanService;
    private readonly ITransactionService _transactionService;
    private readonly IEmailService _emailService;

    public CashierController(
        ISavingsAccountService savingsAccountService,
        ICreditCardService creditCardService,
        ILoanService loanService,
        ITransactionService transactionService,
        IEmailService emailService)
    {
        _savingsAccountService = savingsAccountService;
        _creditCardService = creditCardService;
        _loanService = loanService;
        _transactionService = transactionService;
        _emailService = emailService;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    public async Task<IActionResult> Home()
    {
        var todayCount = await _transactionService.CountTodayByPerformedUserAsync(CurrentUserId);

        return View(new CashierHomeViewModel { TodayOperationsCount = todayCount });
    }

    [HttpGet]
    public IActionResult Deposit() => View(new DepositViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(DepositViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var account = await _savingsAccountService.GetAccountByNumberAsync(model.AccountNumber);

        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "Account not found.");
            return View(model);
        }

        try
        {
            await _transactionService.RegisterTransactionAsync(
                account.Id, TransactionType.Credit, model.Amount, "Cashier deposit", CurrentUserId);

            await _emailService.SendAsync(
                account.OwnerEmail, "Deposit Received",
                EmailTemplates.TransactionNotification(account.OwnerFullName, "Deposit received", model.Amount));

            TempData["SuccessMessage"] = "Deposit completed successfully.";
            return RedirectToAction(nameof(Home));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Withdrawal() => View(new WithdrawalViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdrawal(WithdrawalViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var account = await _savingsAccountService.GetAccountByNumberAsync(model.AccountNumber);

        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "Account not found.");
            return View(model);
        }

        try
        {
            await _transactionService.RegisterTransactionAsync(
                account.Id, TransactionType.Debit, model.Amount, "Cashier withdrawal", CurrentUserId);

            await _emailService.SendAsync(
                account.OwnerEmail, "Withdrawal Processed",
                EmailTemplates.TransactionNotification(account.OwnerFullName, "Withdrawal processed", model.Amount));

            TempData["SuccessMessage"] = "Withdrawal completed successfully.";
            return RedirectToAction(nameof(Home));
        }
        catch (DomainException ex)
        {
            // Rejected attempt logged, balances untouched since the exception was thrown before commit.
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult CardPayment() => View(new CashierCardPaymentViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CardPayment(CashierCardPaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var account = await _savingsAccountService.GetAccountByNumberAsync(model.FromAccountNumber);
        var card = await _creditCardService.GetCardByNumberAsync(model.CardNumber);

        if (account is null || card is null)
        {
            ModelState.AddModelError(string.Empty, "Account or card not found.");
            return View(model);
        }

        try
        {
            await _creditCardService.PayCardAsync(card.Id, account.Id, model.Amount);
            TempData["SuccessMessage"] = "Card payment completed successfully.";
            return RedirectToAction(nameof(Home));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult LoanPayment() => View(new CashierLoanPaymentViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoanPayment(CashierLoanPaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var account = await _savingsAccountService.GetAccountByNumberAsync(model.FromAccountNumber);
        var loan = await _loanService.GetLoanByNumberAsync(model.LoanNumber);

        if (account is null || loan is null)
        {
            ModelState.AddModelError(string.Empty, "Account or loan not found.");
            return View(model);
        }

        try
        {
            await _loanService.PayInstallmentAsync(loan.Id, account.Id, model.Amount);
            TempData["SuccessMessage"] = "Loan payment completed successfully.";
            return RedirectToAction(nameof(Home));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult ThirdPartyTransaction() => View(new ThirdPartyTransactionViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThirdPartyTransaction(ThirdPartyTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var fromAccount = await _savingsAccountService.GetAccountByNumberAsync(model.FromAccountNumber);
        var toAccount = await _savingsAccountService.GetAccountByNumberAsync(model.ToAccountNumber);

        if (fromAccount is null || toAccount is null)
        {
            ModelState.AddModelError(string.Empty, "Origin or destination account not found.");
            return View(model);
        }

        try
        {
            await _transactionService.TransferAsync(
                fromAccount.Id, toAccount.Id, model.Amount, "Cashier third-party transaction", CurrentUserId);

            await _emailService.SendAsync(
                fromAccount.OwnerEmail, "Transaction Notification",
                EmailTemplates.TransactionNotification(fromAccount.OwnerFullName, "Funds sent", model.Amount));

            await _emailService.SendAsync(
                toAccount.OwnerEmail, "Transaction Notification",
                EmailTemplates.TransactionNotification(toAccount.OwnerFullName, "Funds received", model.Amount));

            TempData["SuccessMessage"] = "Transaction completed successfully.";
            return RedirectToAction(nameof(Home));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}