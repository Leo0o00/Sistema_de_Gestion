using Coworking.Infrastructure.Commands.Reservations;

namespace Sistema_de_Gestion.Controllers;

using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Coworking.Infrastructure;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CoworkingDbContext _db;

    public ReservationsController(IMediator mediator, CoworkingDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpGet]
    [Authorize] // Cualquier usuario logueado
    public async Task<IActionResult> GetMyReservations()
    {
        // Extrae userId del token JWT
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized("UserId no encontrado en token.");

        int userId = int.Parse(userIdClaim.Value);

        var reservations = await _db.Reservations
            .Include(r => r.Room)
            .Where(r => r.UserId == userId && !r.IsCancelled)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reservations);
    }

    [HttpPost]
    [Authorize] // Se requiere inicio de sesión
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command)
    {
        // Asegurarnos de que el UserId del command coincide con el del token
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized("UserId no encontrado en token.");

        int userId = int.Parse(userIdClaim.Value);
        if (command.UserId != userId)
            return BadRequest("No puedes crear una reserva para otro usuario.");

        try
        {
            var reservationId = await _mediator.Send(command);
            return Ok(new { reservationId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> EditReservation(int id, [FromBody] EditReservationCommand command)
    {
        // Validar que la reserva sea del usuario que la creó (o Admin)
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);

        if (userIdClaim == null) return Unauthorized("UserId no encontrado en token.");
        
        int userId = int.Parse(userIdClaim.Value);
        bool isAdmin = (roleClaim != null && roleClaim.Value == "Admin");

        // Verificar que la reserva existe y pertenece al userId o el user es admin
        var reservation = await _db.Reservations.FindAsync(id);
        if (reservation == null) return NotFound("Reserva no encontrada.");
        if (reservation.UserId != userId && !isAdmin)
            return Forbid("No tienes permiso para editar esta reserva.");

        // Reemplazar ReservationId en el command
        command = command with { ReservationId = id };

        try
        {
            bool result = await _mediator.Send(command);
            return result ? Ok("Reserva editada con éxito.") : BadRequest("No se pudo editar la reserva.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);

        if (userIdClaim == null) return Unauthorized("UserId no encontrado en token.");

        int userId = int.Parse(userIdClaim.Value);
        bool isAdmin = (roleClaim != null && roleClaim.Value == "Admin");

        var reservation = await _db.Reservations.FindAsync(id);
        if (reservation == null) return NotFound("Reserva no encontrada.");
        if (reservation.UserId != userId && !isAdmin)
            return Forbid("No tienes permiso para cancelar esta reserva.");

        // MediatR command
        var command = new CancelReservationCommand(id);
        try
        {
            bool result = await _mediator.Send(command);
            return result ? Ok("Reserva cancelada con éxito.") : BadRequest("No se pudo cancelar la reserva.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
