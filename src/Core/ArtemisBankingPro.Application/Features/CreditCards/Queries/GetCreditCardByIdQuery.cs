using ArtemisBankingPro.Application.DTOs.CreditCards;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.CreditCards.Queries;

public record GetCreditCardByIdQuery(int Id) : IRequest<CreditCardDto?>;

public class GetCreditCardByIdQueryHandler : IRequestHandler<GetCreditCardByIdQuery, CreditCardDto?>
{
    private readonly ICreditCardService _creditCardService;

    public GetCreditCardByIdQueryHandler(ICreditCardService creditCardService)
    {
        _creditCardService = creditCardService;
    }

    public Task<CreditCardDto?> Handle(GetCreditCardByIdQuery request, CancellationToken cancellationToken) =>
        _creditCardService.GetCardByIdAsync(request.Id);
}