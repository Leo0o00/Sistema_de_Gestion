using Coworking.Application.DTOs;
using Coworking.Infrastructure.Commands.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sistema_de_Gestion.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        
        var command = new RegisterUserCommand(
            Username: request.Username,
            Email: request.Email,
            Password: request.Password
        );

        try
        {
            var newUserId = await _mediator.Send(command);
            return Ok(new { Message = "User registered successfully.", UserId = newUserId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto request)
    {
        
        var command = new LoginUserCommand(
            Username: request.Username,
            Password: request.Password
        );

        try
        {
            string token = await _mediator.Send(command);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{userId}")]
    public async Task<IActionResult> ChangeRole(int userId, [FromBody] ChangeRoleDto request)
    {
        var command = new ChangeUserRoleCommand(userId, request.NewRole);
        try
        {
            bool result = await _mediator.Send(command);
            if (result) return Ok("Role successfully updated.");
            return BadRequest("The role could not be updated.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}