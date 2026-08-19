using FluentValidation;
using MediatR;
using ArtemisBankingPro.Application.Services.Interfaces;

namespace ArtemisBankingPro.Application.Features.CreditCards.Commands;

public record UpdateLimitCommand(int CardId, decimal NewLimit) : IRequest;

public class UpdateLimitCommandValidator : AbstractValidator<UpdateLimitCommand>
{
    public UpdateLimitCommandValidator()
    {
        RuleFor(x => x.CardId).GreaterThan(0);
        RuleFor(x => x.NewLimit).GreaterThan(0);
    }
}

public class UpdateLimitCommandHandler : IRequestHandler<UpdateLimitCommand>
{
    private readonly ICreditCardService _creditCardService;

    public UpdateLimitCommandHandler(ICreditCardService creditCardService)
    {
        _creditCardService = creditCardService;
    }

    public async Task Handle(UpdateLimitCommand request, CancellationToken cancellationToken) =>
        await _creditCardService.UpdateLimitAsync(request.CardId, request.NewLimit);
}