using ArtemisBankingPro.Application.DTOs.Commerce;
using ArtemisBankingPro.Application.Services.Interfaces;
using FluentValidation;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Commerce.Commands;

public record CreateCommerceCommand(
    string Name, string Rnc, string Email, string UserName, string Password) : IRequest<int>;

public class CreateCommerceCommandValidator : AbstractValidator<CreateCommerceCommand>
{
    public CreateCommerceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Rnc).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class CreateCommerceCommandHandler : IRequestHandler<CreateCommerceCommand, int>
{
    private readonly ICommerceService _commerceService;

    public CreateCommerceCommandHandler(ICommerceService commerceService)
    {
        _commerceService = commerceService;
    }

    public Task<int> Handle(CreateCommerceCommand request, CancellationToken cancellationToken) =>
        _commerceService.CreateCommerceAsync(new CreateCommerceDto
        {
            Name = request.Name,
            Rnc = request.Rnc,
            Email = request.Email,
            UserName = request.UserName,
            Password = request.Password
        });
}