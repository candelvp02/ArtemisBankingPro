using ArtemisBankingPro.Application.DTOs.Loans;
using ArtemisBankingPro.Application.Services.Interfaces;
using FluentValidation;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Loans.Commands;

public record AssignLoanCommand(
    string ApplicationUserId, decimal Amount, decimal AnnualInterestRate, int TermMonths) : IRequest<int>;

public class AssignLoanCommandValidator : AbstractValidator<AssignLoanCommand>
{
    public AssignLoanCommandValidator()
    {
        RuleFor(x => x.ApplicationUserId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.AnnualInterestRate).GreaterThan(0);
        RuleFor(x => x.TermMonths).GreaterThan(0);
    }
}

public class AssignLoanCommandHandler : IRequestHandler<AssignLoanCommand, int>
{
    private readonly ILoanService _loanService;

    public AssignLoanCommandHandler(ILoanService loanService)
    {
        _loanService = loanService;
    }

    public Task<int> Handle(AssignLoanCommand request, CancellationToken cancellationToken) =>
        _loanService.AssignLoanAsync(new AssignLoanDto
        {
            ApplicationUserId = request.ApplicationUserId,
            Amount = request.Amount,
            AnnualInterestRate = request.AnnualInterestRate,
            TermMonths = request.TermMonths
        });
}