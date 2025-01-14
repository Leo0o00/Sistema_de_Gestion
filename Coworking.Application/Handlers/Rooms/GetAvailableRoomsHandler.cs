using Coworking.Infrastructure;
using Coworking.Infrastructure.Queries.Rooms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Rooms;

    public class GetAvailableRoomsHandler : IRequestHandler<GetAvailableRoomsQuery, List<Domain.Entities.Rooms>>
    {
        private readonly CoworkingDbContext _context;
        public GetAvailableRoomsHandler(CoworkingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Domain.Entities.Rooms>> Handle(GetAvailableRoomsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Domain.Entities.Rooms> query = _context.Rooms.Where(r => r.IsActive);

            if (request.Capacity.HasValue)
            {
                query = query.Where(r => r.Capacity >= request.Capacity.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Location))
            {
                query = query.Where(r => r.Location.Contains(request.Location));
            }

            return await query.ToListAsync(cancellationToken);
        }
    }
