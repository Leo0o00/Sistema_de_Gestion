using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Infrastructure.Commands.Rooms;
using Coworking.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Rooms;

public class RemoveRoomHandler : IRequestHandler<RemoveRoomCommand, bool>
{
    private readonly IRoomRepository _service;

    public RemoveRoomHandler(IRoomRepository service)
    {
        _service = service;
    }

    public async Task<bool> Handle(RemoveRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _service.GetByIdAsync(request.Id);
        
        if (room == null)
            throw new InvalidOperationException("The room does not exist.");

        return await _service.DeleteAsync(room);
    }
}