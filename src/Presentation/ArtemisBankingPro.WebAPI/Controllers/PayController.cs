using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Application.Features.HermesPay.Commands;
using ArtemisBankingPro.Application.Features.HermesPay.Queries;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebAPI.Controllers;

[ApiController]
[Route("pay")]
[Authorize(Roles = "Administrator,Commerce")]
public class PayController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICommerceService _commerceService;
    private readonly ICurrentUserService _currentUserService;

    public PayController(
        IMediator mediator, ICommerceService commerceService, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _commerceService = commerceService;
        _currentUserService = currentUserService;
    }

    [HttpGet("get-transactions/{commerceId:int}")]
    public async Task<IActionResult> GetTransactions(
        int commerceId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var resolvedCommerceId = await ResolveCommerceIdAsync(commerceId);

        if (resolvedCommerceId is null)
        {
            return Forbid();
        }

        var result = await _mediator.Send(
            new GetCommerceTransactionsQuery(resolvedCommerceId.Value, pageNumber, pageSize));

        return Ok(result);
    }

    [HttpPost("process-payment/{commerceId:int}")]
    public async Task<IActionResult> ProcessPayment(int commerceId, [FromBody] ProcessPaymentRequest request)
    {
        var resolvedCommerceId = await ResolveCommerceIdAsync(commerceId);

        if (resolvedCommerceId is null)
        {
            return Forbid();
        }

        var command = new ProcessPaymentCommand(
            resolvedCommerceId.Value, request.CardNumber, request.ExpirationDate, request.Cvc, request.Amount);

        var result = await _mediator.Send(command);

        return result.Approved ? Ok(result) : UnprocessableEntity(result);
    }

    private async Task<int?> ResolveCommerceIdAsync(int commerceIdFromRoute)
    {
        if (_currentUserService.Role == "Administrator")
        {
            return commerceIdFromRoute;
        }

        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return null;
        }

        var commerce = await _commerceService.GetCommerceByUserIdAsync(userId);

        return commerce?.Id;
    }
}

public record ProcessPaymentRequest(string CardNumber, DateTime ExpirationDate, string Cvc, decimal Amount);