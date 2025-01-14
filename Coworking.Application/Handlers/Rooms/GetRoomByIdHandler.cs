using Coworking.Infrastructure.Queries.Rooms;
using Coworking.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Coworking.Application.Handlers.Rooms;

public class GetRoomByIdHandler : IRequestHandler<GetRoomByIdQuery, Domain.Entities.Rooms>
{
    private readonly IRoomRepository _service;
    
    public GetRoomByIdHandler(IRoomRepository service)
    {
        _service = service;
    }
    public async Task<Domain.Entities.Rooms> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var room = await _service.GetByIdAsync(request.Id);
        if (room == null)
        {
            throw new InvalidOperationException("Room not found.");
        }

        return room;
    }
}