using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Loans;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Loans.Queries;

public record GetLoansQuery(string? Status, string? Cedula, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResult<LoanDto>>;

public class GetLoansQueryHandler : IRequestHandler<GetLoansQuery, PagedResult<LoanDto>>
{
    private readonly ILoanService _loanService;

    public GetLoansQueryHandler(ILoanService loanService)
    {
        _loanService = loanService;
    }

    public Task<PagedResult<LoanDto>> Handle(GetLoansQuery request, CancellationToken cancellationToken) =>
        _loanService.GetLoansAsync(request.Status, request.Cedula, request.PageNumber, request.PageSize);
}