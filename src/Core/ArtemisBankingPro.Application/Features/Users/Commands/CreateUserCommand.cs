using ArtemisBankingPro.Application.DTOs.Users;
using ArtemisBankingPro.Application.Services.Interfaces;
using FluentValidation;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Cedula,
    string Email,
    string UserName,
    string Password,
    string ConfirmPassword,
    string Role,
    decimal? InitialAmount) : IRequest<string>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserService userService)
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Cedula).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");

        RuleFor(x => x.Role)
            .Must(r => r is "Administrator" or "Cashier" or "Client")
            .WithMessage("Role must be Administrator, Cashier or Client.");

        RuleFor(x => x.UserName)
            .MustAsync(async (userName, _) => !await userService.UserNameExistsAsync(userName))
            .WithMessage("Username is already taken.");

        RuleFor(x => x.Email)
            .MustAsync(async (email, _) => !await userService.EmailExistsAsync(email))
            .WithMessage("Email is already registered.");

        RuleFor(x => x.Cedula)
            .MustAsync(async (cedula, _) => !await userService.CedulaExistsAsync(cedula))
            .WithMessage("Cedula is already registered.");
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
{
    private readonly IUserService _userService;

    public CreateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken) =>
        _userService.CreateUserAsync(new CreateUserDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Cedula = request.Cedula,
            Email = request.Email,
            UserName = request.UserName,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            Role = request.Role,
            InitialAmount = request.InitialAmount
        });
}