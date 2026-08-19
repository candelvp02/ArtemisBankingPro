using ArtemisBankingPro.Application.Features.CreditCards.Commands;
using ArtemisBankingPro.Application.Features.CreditCards.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebAPI.Controllers;

[ApiController]
[Route("api/credit-card")]
[Authorize(Roles = "Administrator")]
public class CreditCardController : ControllerBase
{
    private readonly IMediator _mediator;

    public CreditCardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status, [FromQuery] string? cedula,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetCreditCardsQuery(status, cedula, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(AssignCreditCardCommand command)
    {
        var cardId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = cardId }, new { id = cardId });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCreditCardByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:int}/limit")]
    public async Task<IActionResult> UpdateLimit(int id, [FromBody] decimal newLimit)
    {
        await _mediator.Send(new UpdateLimitCommand(id, newLimit));
        return NoContent();
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _mediator.Send(new CancelCardCommand(id));
        return NoContent();
    }
}