using System.Security.Claims;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.WebApp.Filters;
using ArtemisBankingPro.WebApp.ViewModels.Client;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApp.Controllers;

[RoleAuthorize("Client")]
public class ClientController : Controller
{
    private readonly ISavingsAccountService _savingsAccountService;
    private readonly ICreditCardService _creditCardService;
    private readonly ILoanService _loanService;
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ITransactionService _transactionService;

    public ClientController(
        ISavingsAccountService savingsAccountService,
        ICreditCardService creditCardService,
        ILoanService loanService,
        IBeneficiaryService beneficiaryService,
        ITransactionService transactionService)
    {
        _savingsAccountService = savingsAccountService;
        _creditCardService = creditCardService;
        _loanService = loanService;
        _beneficiaryService = beneficiaryService;
        _transactionService = transactionService;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    public async Task<IActionResult> Home()
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        var cards = await _creditCardService.GetCardsByUserIdAsync(CurrentUserId);
        var loan = await _loanService.GetActiveLoanByUserIdAsync(CurrentUserId);

        return View(new ClientHomeViewModel
        {
            Accounts = accounts.Select(a => new AccountSummaryViewModel
            {
                AccountNumber = a.AccountNumber,
                Type = a.Type,
                Balance = a.Balance
            }).ToList(),
            Cards = cards.Select(c => new CardSummaryViewModel
            {
                Id = c.Id,
                MaskedNumber = c.MaskedNumber,
                AvailableCredit = c.AvailableCredit
            }).ToList(),
            Loan = loan is null ? null : new LoanSummaryViewModel
            {
                LoanNumber = loan.LoanNumber,
                MonthlyPayment = loan.MonthlyPayment,
                Status = loan.Status
            }
        });
    }

    public async Task<IActionResult> AccountDetail(string accountNumber)
    {
        var account = await _savingsAccountService.GetAccountByNumberAsync(accountNumber);

        if (account is null || account.ApplicationUserId != CurrentUserId)
        {
            return NotFound();
        }

        var transactions = await _savingsAccountService.GetAccountTransactionsAsync(accountNumber, 1, 50);

        return View(new AccountDetailViewModel
        {
            AccountNumber = account.AccountNumber,
            Type = account.Type,
            Balance = account.Balance,
            Transactions = transactions.Items.Select(t => new TransactionItemViewModel
            {
                Type = t.Type,
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList()
        });
    }

    public async Task<IActionResult> CardDetail(int id)
    {
        var card = await _creditCardService.GetCardByIdAsync(id);

        if (card is null || card.ApplicationUserId != CurrentUserId)
        {
            return NotFound();
        }

        var consumptions = await _creditCardService.GetCardConsumptionsAsync(id);

        return View(new CardDetailViewModel
        {
            Id = card.Id,
            MaskedNumber = card.MaskedNumber,
            CreditLimit = card.CreditLimit,
            CurrentDebt = card.CurrentDebt,
            AvailableCredit = card.AvailableCredit,
            Consumptions = consumptions.Select(c => new ConsumptionItemViewModel
            {
                Amount = c.Amount,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            }).ToList()
        });
    }

    public async Task<IActionResult> LoanDetail()
    {
        var loan = await _loanService.GetActiveLoanByUserIdAsync(CurrentUserId);

        if (loan is null)
        {
            return NotFound();
        }

        return View(new LoanDetailViewModel
        {
            Id = loan.Id,
            LoanNumber = loan.LoanNumber,
            Amount = loan.Amount,
            MonthlyPayment = loan.MonthlyPayment,
            Status = loan.Status,
            Installments = loan.Installments.Select(i => new InstallmentItemViewModel
            {
                Number = i.Number,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Status = i.Status
            }).ToList()
        });
    }

    public async Task<IActionResult> Beneficiaries()
    {
        var beneficiaries = await _beneficiaryService.GetBeneficiariesAsync(CurrentUserId);

        return View(new BeneficiariesViewModel
        {
            Beneficiaries = beneficiaries.Select(b => new BeneficiaryListItemViewModel
            {
                Id = b.Id,
                Alias = b.Alias,
                AccountNumber = b.AccountNumber
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBeneficiary(AddBeneficiaryViewModel model)
    {
        try
        {
            await _beneficiaryService.AddBeneficiaryAsync(CurrentUserId, model.AccountNumber, model.Alias);
            TempData["SuccessMessage"] = "Beneficiary added successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Beneficiaries));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveBeneficiary(int id)
    {
        try
        {
            await _beneficiaryService.RemoveBeneficiaryAsync(id, CurrentUserId);
            TempData["SuccessMessage"] = "Beneficiary removed successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Beneficiaries));
    }

    [HttpGet]
    public async Task<IActionResult> ExpressTransaction()
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);

        return View(new ExpressTransactionViewModel
        {
            AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExpressTransaction(ExpressTransactionViewModel model)
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        model.AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var fromAccount = accounts.FirstOrDefault(a => a.AccountNumber == model.FromAccountNumber);
        var toAccount = await _savingsAccountService.GetAccountByNumberAsync(model.ToAccountNumber);

        if (fromAccount is null || toAccount is null)
        {
            ModelState.AddModelError(string.Empty, "Origin or destination account not found.");
            return View(model);
        }

        try
        {
            await _transactionService.TransferAsync(
                fromAccount.Id, toAccount.Id, model.Amount, "Express transaction", CurrentUserId);

            TempData["SuccessMessage"] = "Transaction completed successfully.";
            return RedirectToAction(nameof(Home));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> TransferToBeneficiary()
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        var beneficiaries = await _beneficiaryService.GetBeneficiariesAsync(CurrentUserId);

        return View(new TransferToBeneficiaryViewModel
        {
            AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList(),
            Beneficiaries = beneficiaries.Select(b => new BeneficiaryListItemViewModel
            {
                Id = b.Id,
                Alias = b.Alias,
                AccountNumber = b.AccountNumber
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TransferToBeneficiary(TransferToBeneficiaryViewModel model)
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        var beneficiaries = await _beneficiaryService.GetBeneficiariesAsync(CurrentUserId);
        model.AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList();
        model.Beneficiaries = beneficiaries.Select(b => new BeneficiaryListItemViewModel
        {
            Id = b.Id,
            Alias = b.Alias,
            AccountNumber = b.AccountNumber
        }).ToList();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var fromAccount = accounts.FirstOrDefault(a => a.AccountNumber == model.FromAccountNumber);
        var beneficiary = beneficiaries.FirstOrDefault(b => b.Id == model.BeneficiaryId);

        if (fromAccount is null || beneficiary is null)
        {
            ModelState.AddModelError(string.Empty, "Origin account or beneficiary not found.");
            return View(model);
        }

        try
        {
            await _transactionService.TransferAsync(
                fromAccount.Id, beneficiary.SavingsAccountId, model.Amount,
                $"Transfer to beneficiary {beneficiary.Alias}", CurrentUserId);

            TempData["SuccessMessage"] = "Transfer completed successfully.";
            return RedirectToAction(nameof(Home));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CardPayment()
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        var cards = await _creditCardService.GetCardsByUserIdAsync(CurrentUserId);

        return View(new CardPaymentViewModel
        {
            AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList(),
            Cards = cards.Select(c => new CardSummaryViewModel
            {
                Id = c.Id,
                MaskedNumber = c.MaskedNumber,
                AvailableCredit = c.AvailableCredit
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CardPayment(CardPaymentViewModel model)
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        var cards = await _creditCardService.GetCardsByUserIdAsync(CurrentUserId);
        model.AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList();
        model.Cards = cards.Select(c => new CardSummaryViewModel
        {
            Id = c.Id,
            MaskedNumber = c.MaskedNumber,
            AvailableCredit = c.AvailableCredit
        }).ToList();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var fromAccount = accounts.FirstOrDefault(a => a.AccountNumber == model.FromAccountNumber);

        if (fromAccount is null)
        {
            ModelState.AddModelError(string.Empty, "Account not found.");
            return View(model);
        }

        try
        {
            await _creditCardService.PayCardAsync(model.CardId, fromAccount.Id, model.Amount);
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
    public async Task<IActionResult> LoanPayment()
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        var loan = await _loanService.GetActiveLoanByUserIdAsync(CurrentUserId);

        return View(new LoanPaymentViewModel
        {
            AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList(),
            LoanNumber = loan?.LoanNumber
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoanPayment(LoanPaymentViewModel model)
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        var loan = await _loanService.GetActiveLoanByUserIdAsync(CurrentUserId);
        model.AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList();
        model.LoanNumber = loan?.LoanNumber;

        if (!ModelState.IsValid || loan is null)
        {
            if (loan is null)
            {
                ModelState.AddModelError(string.Empty, "You do not have an active loan.");
            }

            return View(model);
        }

        var fromAccount = accounts.FirstOrDefault(a => a.AccountNumber == model.FromAccountNumber);

        if (fromAccount is null)
        {
            ModelState.AddModelError(string.Empty, "Account not found.");
            return View(model);
        }

        try
        {
            await _loanService.PayInstallmentAsync(loan.Id, fromAccount.Id, model.Amount);
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
    public async Task<IActionResult> CashAdvance()
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        var cards = await _creditCardService.GetCardsByUserIdAsync(CurrentUserId);

        return View(new CashAdvanceViewModel
        {
            AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList(),
            Cards = cards.Select(c => new CardSummaryViewModel
            {
                Id = c.Id,
                MaskedNumber = c.MaskedNumber,
                AvailableCredit = c.AvailableCredit
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CashAdvance(CashAdvanceViewModel model)
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        var cards = await _creditCardService.GetCardsByUserIdAsync(CurrentUserId);
        model.AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList();
        model.Cards = cards.Select(c => new CardSummaryViewModel
        {
            Id = c.Id,
            MaskedNumber = c.MaskedNumber,
            AvailableCredit = c.AvailableCredit
        }).ToList();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var toAccount = accounts.FirstOrDefault(a => a.AccountNumber == model.ToAccountNumber);

        if (toAccount is null)
        {
            ModelState.AddModelError(string.Empty, "Account not found.");
            return View(model);
        }

        try
        {
            var totalCharged = await _creditCardService.CashAdvanceAsync(model.CardId, toAccount.Id, model.Amount);
            TempData["SuccessMessage"] = $"Cash advance completed. Total charged with interest: {totalCharged:C}.";
            return RedirectToAction(nameof(Home));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> SelfTransfer()
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);

        return View(new SelfTransferViewModel
        {
            AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelfTransfer(SelfTransferViewModel model)
    {
        var accounts = await _savingsAccountService.GetAccountsByUserIdAsync(CurrentUserId);
        model.AvailableAccounts = accounts.Select(a => a.AccountNumber).ToList();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.FromAccountNumber == model.ToAccountNumber)
        {
            ModelState.AddModelError(string.Empty, "Origin and destination accounts must be different.");
            return View(model);
        }

        var fromAccount = accounts.FirstOrDefault(a => a.AccountNumber == model.FromAccountNumber);
        var toAccount = accounts.FirstOrDefault(a => a.AccountNumber == model.ToAccountNumber);

        if (fromAccount is null || toAccount is null)
        {
            ModelState.AddModelError(string.Empty, "One or both accounts do not belong to you.");
            return View(model);
        }

        try
        {
            await _transactionService.TransferAsync(
                fromAccount.Id, toAccount.Id, model.Amount, "Self transfer between own accounts", CurrentUserId);

            TempData["SuccessMessage"] = "Transfer completed successfully.";
            return RedirectToAction(nameof(Home));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}