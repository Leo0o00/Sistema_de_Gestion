using Coworking.Domain.Entities;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Infrastructure.Repositories;
using MediatR;

namespace Coworking.Application.Handlers.Reservations;

public class DeleteReservationHandler : IRequestHandler<DeleteReservationCommand, bool>
{
    private readonly IReservationsRepository _service;

    public DeleteReservationHandler(IReservationsRepository service)
    {
        _service = service;
    }

    public async Task<bool> Handle(DeleteReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _service.GetByIdAsync(request.ReservationId);
        if (reservation == null)
            throw new InvalidOperationException("Reservation not found.");

        // Solo Admin puede eliminar físicamente
        if (request.Role != "Admin")
            throw new UnauthorizedAccessException("You are not allowed to edit this reservation.");

        // Log antes de eliminar
        reservation.AuditLogs.Add(new ReservationAuditLog
        {
            Action = "Deleted",
            Details = $"Reservation {reservation.Id} deleted by Admin {request.UserId}"
        });
        await _service.UpdateAsync(reservation);
        await _service.SaveChangesAsync(cancellationToken);

        // Eliminar
        _service.Delete(reservation);
        await _service.SaveChangesAsync(cancellationToken);

        return true;
    }
}