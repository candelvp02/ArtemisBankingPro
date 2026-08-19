using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Application.Services.Interfaces;
using FluentValidation;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands;

public record ChangeUserStatusCommand(string TargetUserId, bool IsActive) : IRequest;

public class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();
    }
}

public class ChangeUserStatusCommandHandler : IRequestHandler<ChangeUserStatusCommand>
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public ChangeUserStatusCommandHandler(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
    {
        var requestingUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        await _userService.ChangeUserStatusAsync(request.TargetUserId, requestingUserId, request.IsActive);
    }
}