using ArtemisBankingPro.Application.DTOs.CreditCards;
using ArtemisBankingPro.Application.Services.Interfaces;
using FluentValidation;
using MediatR;

namespace ArtemisBankingPro.Application.Features.CreditCards.Commands;

public record AssignCreditCardCommand(string ApplicationUserId, decimal CreditLimit) : IRequest<int>;

public class AssignCreditCardCommandValidator : AbstractValidator<AssignCreditCardCommand>
{
    public AssignCreditCardCommandValidator()
    {
        RuleFor(x => x.ApplicationUserId).NotEmpty();
        RuleFor(x => x.CreditLimit).GreaterThan(0);
    }
}

public class AssignCreditCardCommandHandler : IRequestHandler<AssignCreditCardCommand, int>
{
    private readonly ICreditCardService _creditCardService;

    public AssignCreditCardCommandHandler(ICreditCardService creditCardService)
    {
        _creditCardService = creditCardService;
    }

    public Task<int> Handle(AssignCreditCardCommand request, CancellationToken cancellationToken) =>
        _creditCardService.AssignCardAsync(new AssignCreditCardDto
        {
            ApplicationUserId = request.ApplicationUserId,
            CreditLimit = request.CreditLimit
        });
}