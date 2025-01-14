using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Coworking.Infrastructure.Commands.Users;
using Coworking.Infrastructure.Repositories;
using Coworking.Domain.Entities;
using Org.BouncyCastle.Crypto.Generators;

namespace Coworking.Application.Handlers.Users;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, int>
{
    private readonly IUsersRepository _service;

    public RegisterUserHandler(IUsersRepository service)
    {
        _service = service;
    }

    public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Validar si username o email ya existen
        if (await _service.UsernameExistsAsync(request.Username))
            throw new InvalidOperationException("The username is already in use.");

        if (await _service.EmailExistsAsync(request.Email))
            throw new InvalidOperationException("The email is already in use.");

        // Crear nuevo usuario
        var newUser = new Domain.Entities.Users
        {
            Username = request.Username,
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User" // Se asigna por defecto
        };

        await _service.AddUserAsync(newUser);
        await _service.SaveChangesAsync();

        return newUser.Id;
    }
}