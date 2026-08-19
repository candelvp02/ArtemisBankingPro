using ArtemisBankingPro.Application.Features.SavingsAccounts.Commands;
using ArtemisBankingPro.Application.Features.SavingsAccounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebAPI.Controllers;

[ApiController]
[Route("api/savings-account")]
[Authorize(Roles = "Administrator")]
public class SavingsAccountController : ControllerBase
{
    private readonly IMediator _mediator;

    public SavingsAccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status, [FromQuery] string? type, [FromQuery] string? cedula,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetSavingsAccountsQuery(status, type, cedula, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSecondaryAccountCommand command)
    {
        var accountId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = accountId }, new { id = accountId });
    }

    [HttpGet("{accountNumber}/transactions")]
    public async Task<IActionResult> GetTransactions(
        string accountNumber, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAccountTransactionsQuery(accountNumber, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPatch("{accountId:int}/cancel")]
    public async Task<IActionResult> Cancel(int accountId)
    {
        await _mediator.Send(new CancelAccountCommand(accountId));
        return NoContent();
    }
}