using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.HermesPay.Queries;

public record GetCommerceTransactionsQuery(int CommerceId, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResult<CommerceTransactionDto>>;

public class GetCommerceTransactionsQueryHandler
    : IRequestHandler<GetCommerceTransactionsQuery, PagedResult<CommerceTransactionDto>>
{
    private readonly IHermesPayService _hermesPayService;

    public GetCommerceTransactionsQueryHandler(IHermesPayService hermesPayService)
    {
        _hermesPayService = hermesPayService;
    }

    public Task<PagedResult<CommerceTransactionDto>> Handle(
        GetCommerceTransactionsQuery request, CancellationToken cancellationToken) =>
        _hermesPayService.GetCommerceTransactionsAsync(request.CommerceId, request.PageNumber, request.PageSize);
}