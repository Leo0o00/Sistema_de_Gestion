using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Rooms;
using Coworking.Infrastructure.Repositories;
using MediatR;

namespace Coworking.Application.Handlers.Rooms;

public class CreateRoomHandler : IRequestHandler<CreateRoomCommand, Domain.Entities.Rooms>
{
    private readonly IRoomRepository _service;

    public CreateRoomHandler(IRoomRepository service)
    {
        _service = service;
    }

    public async Task<Domain.Entities.Rooms> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {


        if (await _service.ExistingRoomName(request.Name))
        {
            throw new InvalidOperationException("That room's name is already in use.");
            
        }
        
        var room = new Domain.Entities.Rooms
        {
            Name = request.Name,
            Location = request.Location,
            Capacity = request.Capacity,
            IsActive = request.IsActive
        };

        try
        {

            await _service.AddAsync(room);

            return new Domain.Entities.Rooms
            {
                Name = room.Name,
                Location = room.Location,
                Capacity = room.Capacity,
                IsActive = room.IsActive
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
}