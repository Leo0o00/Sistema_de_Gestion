using MediatR;

namespace Coworking.Infrastructure.Commands.Users;

public record RegisterUserCommand(string Username, string Email, string Password) : IRequest<int>;