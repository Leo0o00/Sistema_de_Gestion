using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Reservations;

public class CancelReservationHandler : IRequestHandler<CancelReservationCommand, bool>
{
    private readonly IReservationsRepository _service;

    public CancelReservationHandler(IReservationsRepository service)
    {
        _service = service;
    }

    public async Task<bool> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _service.GetByIdAsync(request.ReservationId);

        if (reservation == null || reservation.IsCancelled)
            throw new InvalidOperationException("The reservation does not exist or is already cancelled.");

        // Permisos:
        // - Admin: puede cancelar cualquier reserva
        // - User: solo puede cancelar su propia
        if (request.Role != "Admin" && reservation.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not allowed to edit this reservation.");

        
        reservation.IsCancelled = true;
        reservation.UpdatedAt = DateTime.UtcNow;
        

        // Auditoría
        reservation.AuditLogs.Add(new Domain.Entities.ReservationAuditLog
        {
            Action = "Cancelled",
            Details = $"Reservation {reservation.Id} cancelled by user {request.UserId}"
        });

        await _service.UpdateAsync(reservation);
        await _service.SaveChangesAsync(cancellationToken);
        return true;
    }
}