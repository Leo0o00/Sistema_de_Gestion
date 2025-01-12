using MediatR;
using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Reservations.Commands;

public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, int>
{
    private readonly CoworkingDbContext _context;

    public CreateReservationHandler(CoworkingDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        // Validar solapamiento de reservas
        bool overlap = await _context.Reservations.AnyAsync(r =>
                r.RoomId == request.RoomId &&
                !r.IsCanceled &&
                (
                    (request.StartTime >= r.StartTime && request.StartTime < r.EndTime) ||
                    (request.EndTime > r.StartTime && request.EndTime <= r.EndTime)
                ),
            cancellationToken
        );

        if (overlap)
            throw new InvalidOperationException("The room is already booked at that time.");

        var reservation = new Domain.Entities.Reservations
        {
            UserId = request.UserId,
            RoomId = request.RoomId,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        _context.Reservations.Add(reservation);

        // Auditoría
        reservation.AuditLogs.Add(new ReservationAuditLog
        {
            Action = "Created",
            Details = $"Reservation created by user {request.UserId} for room {request.RoomId}."
        });

        await _context.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}