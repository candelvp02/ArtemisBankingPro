using ArtemisBankingPro.Application.Features.Loans.Commands;
using ArtemisBankingPro.Application.Features.Loans.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebAPI.Controllers;

[ApiController]
[Route("api/loan")]
[Authorize(Roles = "Administrator")]
public class LoanController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoanController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status, [FromQuery] string? cedula,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetLoansQuery(status, cedula, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(AssignLoanCommand command)
    {
        var loanId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = loanId }, new { id = loanId });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetLoanByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:int}/rate")]
    public async Task<IActionResult> UpdateRate(int id, [FromBody] decimal newAnnualRate)
    {
        await _mediator.Send(new UpdateRateCommand(id, newAnnualRate));
        return NoContent();
    }
}