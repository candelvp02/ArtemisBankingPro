using FluentValidation;
using MediatR;
using ArtemisBankingPro.Application.Services.Interfaces;

namespace ArtemisBankingPro.Application.Features.Account.Commands;

public record RequestPasswordResetCommand(string Email) : IRequest;

public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand>
{
    private readonly IAuthService _authService;

    public RequestPasswordResetCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken) =>
        await _authService.RequestPasswordResetAsync(request.Email);
}