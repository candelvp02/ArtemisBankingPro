using FluentValidation;
using MediatR;
using ArtemisBankingPro.Application.Services.Interfaces;

namespace ArtemisBankingPro.Application.Features.CreditCards.Commands;

public record CancelCardCommand(int CardId) : IRequest;

public class CancelCardCommandValidator : AbstractValidator<CancelCardCommand>
{
    public CancelCardCommandValidator()
    {
        RuleFor(x => x.CardId).GreaterThan(0);
    }
}

public class CancelCardCommandHandler : IRequestHandler<CancelCardCommand>
{
    private readonly ICreditCardService _creditCardService;

    public CancelCardCommandHandler(ICreditCardService creditCardService)
    {
        _creditCardService = creditCardService;
    }

    public async Task Handle(CancelCardCommand request, CancellationToken cancellationToken) =>
        await _creditCardService.CancelCardAsync(request.CardId);
}