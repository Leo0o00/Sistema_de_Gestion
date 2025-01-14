using MediatR;

namespace Coworking.Infrastructure.Commands.Users;

public record LoginUserCommand(string Username, string Password) : IRequest<string>;