using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Services.Interfaces;
using FluentValidation;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Account.Commands;

public record LoginCommand(string UserName, string Password) : IRequest<LoginResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken) =>
        _authService.LoginAsync(request.UserName, request.Password);
}