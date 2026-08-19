using ArtemisBankingPro.Application.DTOs.SavingsAccounts;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.WebApp.Filters;
using ArtemisBankingPro.WebApp.ViewModels.SavingsAccounts;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApp.Controllers;

[RoleAuthorize("Administrator")]
public class SavingsAccountsController : Controller
{
    private readonly ISavingsAccountService _savingsAccountService;
    private readonly IUserService _userService;

    public SavingsAccountsController(ISavingsAccountService savingsAccountService, IUserService userService)
    {
        _savingsAccountService = savingsAccountService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(string? status, string? type, string? cedula, int pageNumber = 1)
    {
        const int pageSize = 10;

        var result = await _savingsAccountService.GetAccountsAsync(status, type, cedula, pageNumber, pageSize);

        var viewModel = new SavingsAccountListViewModel
        {
            StatusFilter = status,
            TypeFilter = type,
            CedulaFilter = cedula,
            PageNumber = pageNumber,
            TotalPages = result.TotalPages,
            Accounts = result.Items.Select(a => new SavingsAccountListItemViewModel
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                OwnerFullName = a.OwnerFullName,
                OwnerCedula = a.OwnerCedula,
                Type = a.Type,
                Status = a.Status,
                Balance = a.Balance
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateSecondaryAccountViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSecondaryAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = await _userService.GetUserIdByCedulaAsync(model.Cedula);

        if (userId is null)
        {
            ModelState.AddModelError(string.Empty, "No client found with that cedula.");
            return View(model);
        }

        try
        {
            await _savingsAccountService.CreateSecondaryAccountAsync(new CreateSecondaryAccountDto
            {
                ApplicationUserId = userId,
                InitialAmount = model.InitialAmount
            });

            TempData["SuccessMessage"] = "Secondary account created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Detail(string accountNumber)
    {
        var account = await _savingsAccountService.GetAccountByNumberAsync(accountNumber);

        if (account is null)
        {
            return NotFound();
        }

        var transactions = await _savingsAccountService.GetAccountTransactionsAsync(accountNumber, 1, 50);

        return View(new AccountDetailViewModel
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            OwnerFullName = account.OwnerFullName,
            Type = account.Type,
            Status = account.Status,
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _savingsAccountService.CancelAccountAsync(id);
            TempData["SuccessMessage"] = "Account cancelled successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}