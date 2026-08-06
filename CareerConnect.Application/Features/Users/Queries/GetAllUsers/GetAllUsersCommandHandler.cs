using CareerConnect.Application.DTOs.Users;
using CareerConnect.Application.Interfaces;
using MediatR;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CareerConnect.Application.Features.Users.Queries.GetAllUsers;

public sealed class GetAllUsersCommandHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users.Select(u => new UserDto(
            Id: u.Id,
            Email: u.Email,
            Role: u.Role,
            CreatedAt: u.CreatedAt
        ));
    }
}
