using ArtemisBankingPro.Application.DTOs.CreditCards;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.WebApp.Filters;
using ArtemisBankingPro.WebApp.ViewModels.CreditCards;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApp.Controllers;

[RoleAuthorize("Administrator")]
public class CreditCardsController : Controller
{
    private readonly ICreditCardService _creditCardService;
    private readonly IUserService _userService;

    public CreditCardsController(ICreditCardService creditCardService, IUserService userService)
    {
        _creditCardService = creditCardService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(string? status, string? cedula, int pageNumber = 1)
    {
        const int pageSize = 10;

        var result = await _creditCardService.GetCardsAsync(status, cedula, pageNumber, pageSize);

        var viewModel = new CreditCardListViewModel
        {
            StatusFilter = status,
            CedulaFilter = cedula,
            PageNumber = pageNumber,
            TotalPages = result.TotalPages,
            Cards = result.Items.Select(c => new CreditCardListItemViewModel
            {
                Id = c.Id,
                MaskedNumber = c.MaskedNumber,
                OwnerFullName = c.OwnerFullName,
                OwnerCedula = c.OwnerCedula,
                CreditLimit = c.CreditLimit,
                CurrentDebt = c.CurrentDebt,
                Status = c.Status
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Assign() => View(new AssignCardViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignCardViewModel model)
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
            await _creditCardService.AssignCardAsync(new AssignCreditCardDto
            {
                ApplicationUserId = userId,
                CreditLimit = model.CreditLimit
            });

            TempData["SuccessMessage"] = "Credit card assigned successfully.";
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
        var card = await _creditCardService.GetCardByIdAsync(id);

        if (card is null)
        {
            return NotFound();
        }

        var consumptions = await _creditCardService.GetCardConsumptionsAsync(id);

        return View(new CardDetailViewModel
        {
            Id = card.Id,
            MaskedNumber = card.MaskedNumber,
            OwnerFullName = card.OwnerFullName,
            CreditLimit = card.CreditLimit,
            CurrentDebt = card.CurrentDebt,
            AvailableCredit = card.AvailableCredit,
            Status = card.Status,
            Consumptions = consumptions.Select(c => new ConsumptionItemViewModel
            {
                Amount = c.Amount,
                Status = c.Status,
                RejectionReason = c.RejectionReason,
                CreatedAt = c.CreatedAt
            }).ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> EditLimit(int id)
    {
        var card = await _creditCardService.GetCardByIdAsync(id);

        if (card is null)
        {
            return NotFound();
        }

        return View(new EditLimitViewModel { Id = card.Id, CreditLimit = card.CreditLimit });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLimit(EditLimitViewModel model)
    {
        try
        {
            await _creditCardService.UpdateLimitAsync(model.Id, model.CreditLimit);
            TempData["SuccessMessage"] = "Credit limit updated successfully.";
            return RedirectToAction(nameof(Detail), new { id = model.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _creditCardService.CancelCardAsync(id);
            TempData["SuccessMessage"] = "Card cancelled successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}