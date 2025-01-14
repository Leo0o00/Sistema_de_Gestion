using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Rooms;
using Coworking.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Rooms;

public class EditRoomHandler : IRequestHandler<EditRoomCommand, bool>
{
    private readonly IRoomRepository _service;

    public EditRoomHandler(IRoomRepository service)
    {
        _service = service;
    }

    public async Task<bool> Handle(EditRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _service.GetByIdAsync(request.Id);
        if (room == null)
        {
            // throw new InvalidOperationException("The room does not exist.");
            return false;
        }

        room.Name = request.Name;
        room.Location = request.Location;
        room.Capacity = request.Capacity;
        room.IsActive = request.IsActive;

        return await _service.UpdateAsync(room);

    }
}