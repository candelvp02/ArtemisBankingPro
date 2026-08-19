using FluentValidation;
using MediatR;
using ArtemisBankingPro.Application.Services.Interfaces;

namespace ArtemisBankingPro.Application.Features.Loans.Commands;

public record UpdateRateCommand(int LoanId, decimal NewAnnualRate) : IRequest;

public class UpdateRateCommandValidator : AbstractValidator<UpdateRateCommand>
{
    public UpdateRateCommandValidator()
    {
        RuleFor(x => x.LoanId).GreaterThan(0);
        RuleFor(x => x.NewAnnualRate).GreaterThan(0);
    }
}

public class UpdateRateCommandHandler : IRequestHandler<UpdateRateCommand>
{
    private readonly ILoanService _loanService;

    public UpdateRateCommandHandler(ILoanService loanService)
    {
        _loanService = loanService;
    }

    public async Task Handle(UpdateRateCommand request, CancellationToken cancellationToken) =>
        await _loanService.UpdateRateAsync(request.LoanId, request.NewAnnualRate);
}