using MediatR;

namespace Coworking.Infrastructure.Commands.Users;

public record ChangeUserRoleCommand(int UserId, string NewRole) : IRequest<bool>;