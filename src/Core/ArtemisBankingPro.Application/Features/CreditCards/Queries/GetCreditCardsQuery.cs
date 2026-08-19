using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.CreditCards;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.CreditCards.Queries;

public record GetCreditCardsQuery(string? Status, string? Cedula, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResult<CreditCardDto>>;

public class GetCreditCardsQueryHandler : IRequestHandler<GetCreditCardsQuery, PagedResult<CreditCardDto>>
{
    private readonly ICreditCardService _creditCardService;

    public GetCreditCardsQueryHandler(ICreditCardService creditCardService)
    {
        _creditCardService = creditCardService;
    }

    public Task<PagedResult<CreditCardDto>> Handle(GetCreditCardsQuery request, CancellationToken cancellationToken) =>
        _creditCardService.GetCardsAsync(request.Status, request.Cedula, request.PageNumber, request.PageSize);
}