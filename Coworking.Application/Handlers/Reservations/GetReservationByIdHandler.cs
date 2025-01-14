using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Queries.Reservations;
using MediatR;

namespace Coworking.Application.Handlers.Reservations;

public class GetReservationByIdHandler : IRequestHandler<GetReservationByIdQuery, Domain.Entities.Reservations>
{
    private readonly CoworkingDbContext _dbContext;
    public GetReservationByIdHandler(CoworkingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Domain.Entities.Reservations> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var reservation = await _dbContext.Reservations.FindAsync(new object[] { request.Id }, cancellationToken);
        if (reservation == null)
        {
            throw new Exception("Reservation not found");
        }
        return new Domain.Entities.Reservations
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
        };
    }
}