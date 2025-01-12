using Coworking.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Reservations.Commands;

public class CancelReservationHandler : IRequestHandler<CancelReservationCommand, bool>
{
    private readonly CoworkingDbContext _context;

    public CancelReservationHandler(CoworkingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.AuditLogs)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken);

        if (reservation == null || reservation.IsCanceled)
            throw new InvalidOperationException("The reservation does not exist or is already cancelled.");

        reservation.IsCanceled = true;
        reservation.UpdatedAt = DateTime.UtcNow;

        // Auditoría
        reservation.AuditLogs.Add(new Domain.Entities.ReservationAuditLog
        {
            Action = "Cancelled",
            Details = $"Reservation {reservation.Id} cancelled."
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}