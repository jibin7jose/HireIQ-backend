using CareerConnect.Application.DTOs.Users;
using MediatR;
using System.Collections.Generic;

namespace CareerConnect.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;
