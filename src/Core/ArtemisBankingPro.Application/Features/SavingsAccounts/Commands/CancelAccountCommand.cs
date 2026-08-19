using FluentValidation;
using MediatR;
using ArtemisBankingPro.Application.Services.Interfaces;

namespace ArtemisBankingPro.Application.Features.SavingsAccounts.Commands;

public record CancelAccountCommand(int AccountId) : IRequest;

public class CancelAccountCommandValidator : AbstractValidator<CancelAccountCommand>
{
    public CancelAccountCommandValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0);
    }
}

public class CancelAccountCommandHandler : IRequestHandler<CancelAccountCommand>
{
    private readonly ISavingsAccountService _savingsAccountService;

    public CancelAccountCommandHandler(ISavingsAccountService savingsAccountService)
    {
        _savingsAccountService = savingsAccountService;
    }

    public async Task Handle(CancelAccountCommand request, CancellationToken cancellationToken) =>
        await _savingsAccountService.CancelAccountAsync(request.AccountId);
}