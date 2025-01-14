using MediatR;
using Coworking.Infrastructure.Commands.Users;
using Coworking.Infrastructure.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coworking.Application.Handlers.Users;

public class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand, bool>
{
    private readonly IUsersRepository _service;

    public ChangeUserRoleHandler(IUsersRepository service)
    {
        _service = service;
    }

    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _service.GetByIdAsync(request.UserId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (request.NewRole != "Admin" && request.NewRole != "User")
            throw new InvalidOperationException("The role must be ‘Admin’ or ‘User’.");

        user.Role = request.NewRole == "Admin"
            ? "Admin"
            : "User";

        await _service.UpdateUserAsync(user);
        await _service.SaveChangesAsync();

        return true;
    }
}