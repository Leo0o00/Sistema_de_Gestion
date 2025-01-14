
using Coworking.Application.DTOs.Rooms;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Rooms;
using Coworking.Infrastructure.Queries.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace Sistema_de_Gestion.Controllers;


[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;
    

    public RoomsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    // GET: api/rooms?capacity=AnyNumber&location=SomePlace
    [HttpGet]
    public async Task<ActionResult<List<Rooms>>> GetAvailableRooms([FromQuery] int? capacity, [FromQuery] string? location)
    {
        var query = new GetAvailableRoomsQuery(capacity, location);
        var rooms = await _mediator.Send(query);
        return Ok(rooms);
    }

    // GET: api/rooms/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Rooms>> GetById(int id)
    {
        var query = new GetRoomByIdQuery(id);
        try
        {
            var room = await _mediator.Send(query);
            return Ok(room);

        }
        catch (InvalidOperationException e)
        {
            return NotFound("Room not found.");
        }
    }
    
    // Crear sala (Solo Admin)
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<Rooms>> CreateRoom(CreateRoomCommand command)
    {
        // if (string.IsNullOrWhiteSpace(room.Name)) 
        //     return BadRequest("Room name is required.");
        //
        // await _db.Rooms.AddAsync(room);
        // await _db.SaveChangesAsync();
        // return Ok(new { message = "Room successfully created.", roomId = room.Id });

        try
        {
            var room = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);

        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
        

    }


    // Editar sala (Solo Admin)
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> EditRoom(int id, [FromBody] UpdateRoomDto request)
    {
        var command = new EditRoomCommand(
            Id: id,
            Name: request.Name,
            Location: request.Location,
            Capacity: request.Capacity,
            IsActive: request.isActive);
        
        var existingRoom = await _mediator.Send(command);
        
        if (!existingRoom) return NotFound("Room not found.");
        
        return Ok("Room successfully updated.");
    }

    // Eliminar sala (Solo Admin) - O inactivarla
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        
        var room = await _mediator.Send(new RemoveRoomCommand(id));
        if (!room) return NotFound("Room not found.");
        
        return Ok("Room successfully removed.");
    }
}
