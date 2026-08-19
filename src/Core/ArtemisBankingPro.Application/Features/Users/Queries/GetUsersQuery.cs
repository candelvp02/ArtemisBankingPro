using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.DTOs.Users;
using ArtemisBankingPro.Application.Services.Interfaces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Queries;

public record GetUsersQuery(string? Role, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResult<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserService _userService;

    public GetUsersQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken) =>
        _userService.GetUsersAsync(request.Role, request.PageNumber, request.PageSize);
}