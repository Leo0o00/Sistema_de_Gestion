using Coworking.Infrastructure;
using Coworking.Infrastructure.Queries.Rooms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Rooms;

using Microsoft.Extensions.Caching.Memory;

public class GetAvailableRoomsCachedHandler : IRequestHandler<GetAvailableRoomsQuery, List<Domain.Entities.Rooms>>
{
    private readonly CoworkingDbContext _context;
    private readonly IMemoryCache _cache;

    public GetAvailableRoomsCachedHandler(CoworkingDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<Domain.Entities.Rooms>> Handle(GetAvailableRoomsQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"AvailableRooms_{request.Capacity}_{request.Location}";
        if (_cache.TryGetValue(cacheKey, out List<Domain.Entities.Rooms>? rooms)) return rooms!;
        IQueryable<Domain.Entities.Rooms> query = _context.Rooms.Where(r => r.IsActive);

        if (request.Capacity.HasValue)
            query = query.Where(r => r.Capacity >= request.Capacity.Value);

        if (!string.IsNullOrWhiteSpace(request.Location))
            query = query.Where(r => r.Location.Contains(request.Location));

        rooms = await query.ToListAsync(cancellationToken);

        _cache.Set(cacheKey, rooms, TimeSpan.FromMinutes(5));

        return rooms!;
    }
}
