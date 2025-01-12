
using Microsoft.AspNetCore.Mvc;
using Coworking.Infrastructure;
using Coworking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;


namespace Sistema_de_Gestion.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly CoworkingDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(CoworkingDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Users user)
    {
        // Se valida si ya existe ese username
        bool userExists = await _db.Users.AnyAsync(u => u.Username == user.Username);
        if (userExists)
            return BadRequest("El usuario ya existe.");

        // Se hashea el password con BCrypt
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();

        return Ok("Usuario registrado con éxito.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Users loginData)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == loginData.Username);
        if (user == null)
            return Unauthorized("User not found.");

        bool validPassword = BCrypt.Net.BCrypt.Verify(loginData.Password, user.Password);
        if (!validPassword)
            return Unauthorized("Incorrect Password.");

        // Se genera el token JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(4),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return Ok(new { token = jwt, role = user.Role });
    }
}
