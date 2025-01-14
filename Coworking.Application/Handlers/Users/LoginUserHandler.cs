using MediatR;
using Coworking.Infrastructure.Commands.Users;
using Coworking.Infrastructure.Repositories;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Coworking.Application.Handlers.Users;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, string>
{
    private readonly IUsersRepository _service;
    private readonly IConfiguration _configuration;

    public LoginUserHandler(IUsersRepository service, IConfiguration configuration)
    {
        _service = service;
        _configuration = configuration;
    }

    public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _service.GetByUsernameAsync(request.Username);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        bool validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        if (!validPassword)
            throw new UnauthorizedAccessException("Wrong password.");

        // Generar token JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(4),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
            
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}