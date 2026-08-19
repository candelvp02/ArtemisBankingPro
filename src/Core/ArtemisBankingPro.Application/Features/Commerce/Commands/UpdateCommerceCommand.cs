using FluentValidation;
using MediatR;
using ArtemisBankingPro.Application.DTOs.Commerce;
using ArtemisBankingPro.Application.Services.Interfaces;

namespace ArtemisBankingPro.Application.Features.Commerce.Commands;

public record UpdateCommerceCommand(int Id, string Name, string Email) : IRequest;

public class UpdateCommerceCommandValidator : AbstractValidator<UpdateCommerceCommand>
{
    public UpdateCommerceCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class UpdateCommerceCommandHandler : IRequestHandler<UpdateCommerceCommand>
{
    private readonly ICommerceService _commerceService;

    public UpdateCommerceCommandHandler(ICommerceService commerceService)
    {
        _commerceService = commerceService;
    }

    public async Task Handle(UpdateCommerceCommand request, CancellationToken cancellationToken) =>
        await _commerceService.UpdateCommerceAsync(new UpdateCommerceDto
        {
            Id = request.Id,
            Name = request.Name,
            Email = request.Email
        });
}