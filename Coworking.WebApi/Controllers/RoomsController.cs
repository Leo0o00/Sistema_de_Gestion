
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Coworking.Application.Rooms.Queries;
using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace Sistema_de_Gestion.Controllers;


[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CoworkingDbContext _db;

    public RoomsController(IMediator mediator, CoworkingDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }
    
    // GET: api/rooms?capacity=AnyNumber&location=SomePlace
    [HttpGet]
    public async Task<ActionResult<List<Rooms>>> GetAvailableRooms([FromQuery] int? capacity, [FromQuery] string? location)
    {
        var query = new GetAvailableRoomsQuery(capacity, location);
        var rooms = await _mediator.Send(query);
        return Ok(rooms);
    }

    // Crear sala (Solo Admin)
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateRoom([FromBody] Rooms room)
    {
        if (string.IsNullOrWhiteSpace(room.Name)) 
            return BadRequest("Room name is required.");

        await _db.Rooms.AddAsync(room);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Room successfully created.", roomId = room.Id });
    }

    // Editar sala (Solo Admin)
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> EditRoom(int id, [FromBody] Rooms updatedRoom)
    {
        var existing = await _db.Rooms.FindAsync(id);
        if (existing == null) return NotFound("Room not found.");

        existing.Name = updatedRoom.Name;
        existing.Location = updatedRoom.Location;
        existing.Capacity = updatedRoom.Capacity;
        existing.IsActive = updatedRoom.IsActive;

        await _db.SaveChangesAsync();
        return Ok("Room successfully updated.");
    }

    // Eliminar sala (Solo Admin) - O inactivarla
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room == null) return NotFound("Room not found.");

        // Aquí se podría eliminar tal cual o solo marcarla como IsActive = false
        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();
        return Ok("Room successfully removed.");
    }
}
