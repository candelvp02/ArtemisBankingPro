using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.SavingsAccounts;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.SavingsAccounts.Queries;

public record GetAccountTransactionsQuery(string AccountNumber, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResult<TransactionDto>>;

public class GetAccountTransactionsQueryHandler
    : IRequestHandler<GetAccountTransactionsQuery, PagedResult<TransactionDto>>
{
    private readonly ISavingsAccountService _savingsAccountService;

    public GetAccountTransactionsQueryHandler(ISavingsAccountService savingsAccountService)
    {
        _savingsAccountService = savingsAccountService;
    }

    public Task<PagedResult<TransactionDto>> Handle(
        GetAccountTransactionsQuery request, CancellationToken cancellationToken) =>
        _savingsAccountService.GetAccountTransactionsAsync(
            request.AccountNumber, request.PageNumber, request.PageSize);
}