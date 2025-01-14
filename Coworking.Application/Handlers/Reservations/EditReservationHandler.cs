using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Reservations;

public class EditReservationHandler : IRequestHandler<EditReservationCommand, bool>
{
    private readonly CoworkingDbContext _context;

    public EditReservationHandler(CoworkingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(EditReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.AuditLogs)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken);

        if (reservation == null || reservation.IsCancelled)
            throw new InvalidOperationException("The reservation does not exist or is cancelled.");

        // Validar solapamiento
        bool overlap = await _context.Reservations.AnyAsync(r =>
                r.Id != request.ReservationId &&
                r.RoomId == reservation.RoomId &&
                !r.IsCancelled &&
                (
                    (request.StartTime >= r.StartTime && request.StartTime < r.EndTime) ||
                    (request.EndTime > r.StartTime && request.EndTime <= r.EndTime)
                ),
            cancellationToken
        );

        if (overlap)
            throw new InvalidOperationException("You cannot edit the reservation, there is an overlap.");

        reservation.StartTime = request.StartTime;
        reservation.EndTime = request.EndTime;
        reservation.UpdatedAt = DateTime.UtcNow;

        // Auditoría
        reservation.AuditLogs.Add(new Domain.Entities.ReservationAuditLog
        {
            Action = "Edited",
            Details = $"Reservation {reservation.Id} edited."
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}