using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Commerce;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Commerce.Queries;

public record GetCommercesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<CommerceDto>>;

public class GetCommercesQueryHandler : IRequestHandler<GetCommercesQuery, PagedResult<CommerceDto>>
{
    private readonly ICommerceService _commerceService;

    public GetCommercesQueryHandler(ICommerceService commerceService)
    {
        _commerceService = commerceService;
    }

    public Task<PagedResult<CommerceDto>> Handle(GetCommercesQuery request, CancellationToken cancellationToken) =>
        _commerceService.GetCommercesAsync(request.PageNumber, request.PageSize);
}