using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.WebApp.Filters;
using ArtemisBankingPro.WebApp.ViewModels.Loans;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApp.Controllers;

[RoleAuthorize("Administrator")]
public class LoansController : Controller
{
    private readonly ILoanService _loanService;
    private readonly IUserService _userService;

    public LoansController(ILoanService loanService, IUserService userService)
    {
        _loanService = loanService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(string? status, string? cedula, int pageNumber = 1)
    {
        const int pageSize = 10;

        var result = await _loanService.GetLoansAsync(status, cedula, pageNumber, pageSize);

        var viewModel = new LoanListViewModel
        {
            StatusFilter = status,
            CedulaFilter = cedula,
            PageNumber = pageNumber,
            TotalPages = result.TotalPages,
            Loans = result.Items.Select(l => new LoanListItemViewModel
            {
                Id = l.Id,
                LoanNumber = l.LoanNumber,
                OwnerFullName = l.OwnerFullName,
                OwnerCedula = l.OwnerCedula,
                Amount = l.Amount,
                MonthlyPayment = l.MonthlyPayment,
                Status = l.Status
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Assign() => View(new AssignLoanViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignLoanViewModel model)
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
            await _loanService.AssignLoanAsync(new Application.DTOs.Loans.AssignLoanDto
            {
                ApplicationUserId = userId,
                Amount = model.Amount,
                AnnualInterestRate = model.AnnualInterestRate,
                TermMonths = model.TermMonths
            });

            TempData["SuccessMessage"] = "Loan assigned successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);

        if (loan is null)
        {
            return NotFound();
        }

        return View(new LoanDetailViewModel
        {
            Id = loan.Id,
            LoanNumber = loan.LoanNumber,
            OwnerFullName = loan.OwnerFullName,
            Amount = loan.Amount,
            AnnualInterestRate = loan.AnnualInterestRate,
            TermMonths = loan.TermMonths,
            MonthlyPayment = loan.MonthlyPayment,
            Status = loan.Status,
            Installments = loan.Installments.Select(i => new InstallmentItemViewModel
            {
                Number = i.Number,
                DueDate = i.DueDate,
                PrincipalAmount = i.PrincipalAmount,
                InterestAmount = i.InterestAmount,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Status = i.Status
            }).ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> EditRate(int id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);

        if (loan is null)
        {
            return NotFound();
        }

        return View(new EditRateViewModel { Id = loan.Id, NewAnnualRate = loan.AnnualInterestRate });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRate(EditRateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _loanService.UpdateRateAsync(model.Id, model.NewAnnualRate);
            TempData["SuccessMessage"] = "Interest rate updated successfully.";
            return RedirectToAction(nameof(Detail), new { id = model.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}