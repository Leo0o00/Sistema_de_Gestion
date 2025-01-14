using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Queries.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Reservations;

public class GetAllReservationsHandler : IRequestHandler<GetAllReservationsQuery, IEnumerable<Domain.Entities.Reservations>>
{
    private readonly CoworkingDbContext _dbContext;
    
    public GetAllReservationsHandler(CoworkingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IEnumerable<Domain.Entities.Reservations>> Handle(GetAllReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservations = await _dbContext.Reservations.Include(reservations => reservations.Room)
            .Include(reservations => reservations.User).ToListAsync(cancellationToken);
        return reservations.Select(reservation => new Domain.Entities.Reservations
        {
            Id = reservation.Id,
            Room = new Domain.Entities.Rooms
            {
                Name = reservation.Room.Name,
                Location = reservation.Room.Location
            },
            User = new Domain.Entities.Users
            {
                Username = reservation.User.Username,
                Email = reservation.User.Email
            },
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            IsCancelled = reservation.IsCancelled
        });
    }
}