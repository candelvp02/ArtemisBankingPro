using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.SavingsAccounts;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.SavingsAccounts.Queries;

public record GetSavingsAccountsQuery(
    string? Status, string? Type, string? Cedula, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResult<SavingsAccountDto>>;

public class GetSavingsAccountsQueryHandler
    : IRequestHandler<GetSavingsAccountsQuery, PagedResult<SavingsAccountDto>>
{
    private readonly ISavingsAccountService _savingsAccountService;

    public GetSavingsAccountsQueryHandler(ISavingsAccountService savingsAccountService)
    {
        _savingsAccountService = savingsAccountService;
    }

    public Task<PagedResult<SavingsAccountDto>> Handle(
        GetSavingsAccountsQuery request, CancellationToken cancellationToken) =>
        _savingsAccountService.GetAccountsAsync(
            request.Status, request.Type, request.Cedula, request.PageNumber, request.PageSize);
}