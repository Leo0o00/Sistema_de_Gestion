using Coworking.Infrastructure;
using Coworking.Infrastructure.Queries.Rooms;
using Coworking.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Rooms;

public class GetAllRoomsHandler : IRequestHandler<GetAllRoomsQuery, List<Domain.Entities.Rooms>>
{
    private readonly IRoomRepository _service;
    
    public GetAllRoomsHandler(IRoomRepository service)
    {
        _service = service;
    }
    public async Task<List<Domain.Entities.Rooms>> Handle(GetAllRoomsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetAllAsync();
    }
}