using System.Security.Claims;
using Coworking.Application.DTOs.Reservations;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Infrastructure.Queries.Reservations;
using Coworking.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Coworking.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Sistema_de_Gestion.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;

    public ReservationsController(IMediator mediator, IEmailService emailService)
    {
        _mediator = mediator;
        _emailService = emailService;
    }

    [HttpGet]
    [Authorize] // Cualquier usuario logueado
    public async Task<IActionResult> GetMyReservations()
    {
        // Extraer userId y role
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null || roleClaim == null)
            return Unauthorized("Your identity could not be determined.");

        int userId = int.Parse(userIdClaim.Value);
        string role = roleClaim.Value;

        var query = new GetAllReservationsQuery(userId, role);
        var reservations = await _mediator.Send(query);
        var response = reservations.Select(r => new ReservationDto
        {
            Id = r.Id,
            UserId = r.UserId,
            RoomId = r.RoomId,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            IsCancelled = r.IsCancelled
        }).ToList();
        
        return Ok(response);
    }

    [HttpPost]
    [Authorize] // Se requiere inicio de sesión
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand request)
    {
        // Asegurarnos de que el UserId del request coincide con el del token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null || roleClaim == null)
            return Unauthorized("Your identity could not be determined.");

        int userId = int.Parse(userIdClaim.Value);
        
        if (request.UserId != userId)
            return BadRequest("You cannot create a reservation for another user.");

        var command = new CreateReservationCommand(
            request.UserId,
            request.RoomId,
            request.StartTime,
            request.EndTime);

        try
        {
            await _mediator.Send(command);
            return Ok("Reservation created successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> EditReservation(int id, [FromBody] UpdateReservationDto request)
    {
        // Validar que la reserva sea del usuario que la creó (o Admin)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null || roleClaim == null) return Unauthorized("Your identity could not be determined.");
        
        int userId = int.Parse(userIdClaim.Value);
        string role = roleClaim.Value;
        
        var command = new EditReservationCommand(
            ReservationId: id,
            StartTime: request.StartTime,
            EndTime: request.EndTime,
            UserId: userId,
            Role: role
        );

        try
        {
            bool result = await _mediator.Send(command);
            if (result) return Ok("Reservation edited successfully.");
            return BadRequest("The reservation could not be edited.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        if (userIdClaim == null || roleClaim == null)
            return Unauthorized("Your identity could not be determined.");

        int userId = int.Parse(userIdClaim.Value);
        string role = roleClaim.Value;

        var command = new CancelReservationCommand(
            ReservationId: id,
            UserId: userId,
            Role: role
        );

        try
        {
            bool result = await _mediator.Send(command);
            if (result) return Ok("Reservation cancelled successfully.");
            return BadRequest("The reservation could not be cancelled.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // Eliminar físicamente (solo Admin)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        if (userIdClaim == null || roleClaim == null)
            return Unauthorized("Your identity could not be determined.");

        int userId = int.Parse(userIdClaim.Value);
        string role = roleClaim.Value;

        var command = new DeleteReservationCommand(id, userId, role);

        try
        {
            bool result = await _mediator.Send(command);
            if (result) return Ok("Reservation successfully removed.");
            return BadRequest("The reservation could not be removed.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
