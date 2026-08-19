using ArtemisBankingPro.Application.DTOs.Users;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Queries;

public record GetUserByIdQuery(string Id) : IRequest<UserDto?>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserService _userService;

    public GetUserByIdQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken) =>
        _userService.GetUserByIdAsync(request.Id);
}