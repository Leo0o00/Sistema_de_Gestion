using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Reservations;

public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, Domain.Entities.Reservations>
{
    private readonly IReservationsRepository _service;

    public CreateReservationHandler(IReservationsRepository service)
    {
        _service = service;
    }

    public async Task<Domain.Entities.Reservations> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        // Validar solapamiento de reservas
        bool overlap = await _service.FindOverlap(request.RoomId, request.StartTime, request.EndTime, cancellationToken);

        if (overlap)
            throw new InvalidOperationException("The room is already booked at that time.");

        var reservation = new Domain.Entities.Reservations
        {
            UserId = request.UserId,
            RoomId = request.RoomId,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        await _service.AddAsync(reservation);

        // Auditoría
        reservation.AuditLogs.Add(new ReservationAuditLog
        {
            Action = "Created",
            Details = $"Reservation created by user {request.UserId} for room {request.RoomId}."
        });

        await _service.SaveChangesAsync(cancellationToken);

        return new Domain.Entities.Reservations
        {
            UserId = reservation.UserId,
            RoomId = reservation.RoomId,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        };;
    }
}