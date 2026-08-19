using FluentValidation;
using MediatR;
using ArtemisBankingPro.Application.Services.Interfaces;

namespace ArtemisBankingPro.Application.Features.Commerce.Commands;

public record ChangeCommerceStatusCommand(int Id, bool IsActive) : IRequest;

public class ChangeCommerceStatusCommandValidator : AbstractValidator<ChangeCommerceStatusCommand>
{
    public ChangeCommerceStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ChangeCommerceStatusCommandHandler : IRequestHandler<ChangeCommerceStatusCommand>
{
    private readonly ICommerceService _commerceService;

    public ChangeCommerceStatusCommandHandler(ICommerceService commerceService)
    {
        _commerceService = commerceService;
    }

    public async Task Handle(ChangeCommerceStatusCommand request, CancellationToken cancellationToken) =>
        await _commerceService.ChangeStatusAsync(request.Id, request.IsActive);
}