using ArtemisBankingPro.Application.DTOs.Commerce;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Commerce.Queries;

public record GetCommerceByIdQuery(int Id) : IRequest<CommerceDto?>;

public class GetCommerceByIdQueryHandler : IRequestHandler<GetCommerceByIdQuery, CommerceDto?>
{
    private readonly ICommerceService _commerceService;

    public GetCommerceByIdQueryHandler(ICommerceService commerceService)
    {
        _commerceService = commerceService;
    }

    public Task<CommerceDto?> Handle(GetCommerceByIdQuery request, CancellationToken cancellationToken) =>
        _commerceService.GetCommerceByIdAsync(request.Id);
}