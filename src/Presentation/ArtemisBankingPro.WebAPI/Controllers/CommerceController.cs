using ArtemisBankingPro.Application.Features.Commerce.Commands;
using ArtemisBankingPro.Application.Features.Commerce.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebAPI.Controllers;

[ApiController]
[Route("api/commerce")]
[Authorize(Roles = "Administrator")]
public class CommerceController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommerceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetCommercesQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCommerceByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCommerceCommand command)
    {
        var commerceId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = commerceId }, new { id = commerceId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCommerceCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Route id does not match command id.");
        }

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] bool isActive)
    {
        await _mediator.Send(new ChangeCommerceStatusCommand(id, isActive));
        return NoContent();
    }
}