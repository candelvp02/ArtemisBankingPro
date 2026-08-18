using FluentValidation;
using MediatR;
using ArtemisBankingPro.Application.Services.Interfaces;

namespace ArtemisBankingPro.Application.Features.Account.Commands;

public record ConfirmAccountCommand(string UserId, string Token) : IRequest;

public class ConfirmAccountCommandValidator : AbstractValidator<ConfirmAccountCommand>
{
    public ConfirmAccountCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class ConfirmAccountCommandHandler : IRequestHandler<ConfirmAccountCommand>
{
    private readonly IAuthService _authService;

    public ConfirmAccountCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task Handle(ConfirmAccountCommand request, CancellationToken cancellationToken) =>
        await _authService.ConfirmAccountAsync(request.UserId, request.Token);
}