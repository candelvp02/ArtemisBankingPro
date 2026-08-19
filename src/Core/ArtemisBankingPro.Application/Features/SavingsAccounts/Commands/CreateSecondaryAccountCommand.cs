using ArtemisBankingPro.Application.DTOs.SavingsAccounts;
using ArtemisBankingPro.Application.Services.Interfaces;
using FluentValidation;
using MediatR;

namespace ArtemisBankingPro.Application.Features.SavingsAccounts.Commands;

public record CreateSecondaryAccountCommand(string ApplicationUserId, decimal InitialAmount) : IRequest<int>;

public class CreateSecondaryAccountCommandValidator : AbstractValidator<CreateSecondaryAccountCommand>
{
    public CreateSecondaryAccountCommandValidator()
    {
        RuleFor(x => x.ApplicationUserId).NotEmpty();
        RuleFor(x => x.InitialAmount).GreaterThanOrEqualTo(0);
    }
}

public class CreateSecondaryAccountCommandHandler : IRequestHandler<CreateSecondaryAccountCommand, int>
{
    private readonly ISavingsAccountService _savingsAccountService;

    public CreateSecondaryAccountCommandHandler(ISavingsAccountService savingsAccountService)
    {
        _savingsAccountService = savingsAccountService;
    }

    public Task<int> Handle(CreateSecondaryAccountCommand request, CancellationToken cancellationToken) =>
        _savingsAccountService.CreateSecondaryAccountAsync(new CreateSecondaryAccountDto
        {
            ApplicationUserId = request.ApplicationUserId,
            InitialAmount = request.InitialAmount
        });
}