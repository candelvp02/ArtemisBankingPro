using ArtemisBankingPro.Application.DTOs.Loans;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Loans.Queries;

public record GetLoanByIdQuery(int Id) : IRequest<LoanDto?>;

public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanDto?>
{
    private readonly ILoanService _loanService;

    public GetLoanByIdQueryHandler(ILoanService loanService)
    {
        _loanService = loanService;
    }

    public Task<LoanDto?> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken) =>
        _loanService.GetLoanByIdAsync(request.Id);
}