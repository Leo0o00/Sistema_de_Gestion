using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Reservations;

public class EditReservationHandler : IRequestHandler<EditReservationCommand, bool>
{
    private readonly IReservationsRepository _service;

    public EditReservationHandler(IReservationsRepository service)
    {
        _service = service;
    }

    public async Task<bool> Handle(EditReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _service.GetByIdAsync(request.ReservationId);
        if (reservation == null)
            throw new InvalidOperationException("Reservation not found.");

        // Validar permisos:
        // - Admin: puede editar cualquier reserva
        // - User: solo puede editar sus reservas
        if (request.Role != "Admin" && reservation.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not allowed to edit this reservation.");

        
        // Validar solapamiento
        bool overlap =
            await _service.FindOverlap(reservation.RoomId, request.StartTime, request.EndTime, cancellationToken);

        if (overlap)
            throw new InvalidOperationException("You cannot edit the reservation, there is an overlap.");

        reservation.StartTime = request.StartTime;
        reservation.EndTime = request.EndTime;
        reservation.UpdatedAt = DateTime.UtcNow;
        

        // Auditoría
        reservation.AuditLogs.Add(new Domain.Entities.ReservationAuditLog
        {
            Action = "Edited",
            Details = $"Reservation {reservation.Id} edited by user {request.UserId}"
        });

        await _service.UpdateAsync(reservation);
        await _service.SaveChangesAsync(cancellationToken);
        return true;
    }
}