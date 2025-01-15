using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Infrastructure.Repositories;
using Coworking.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Reservations;

public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, Domain.Entities.Reservations>
{
    private readonly IReservationsRepository _service;
    private readonly IEmailService _emailService;

    public CreateReservationHandler(IReservationsRepository service, IEmailService emailService)
    {
        _service = service;
        _emailService = emailService;
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
        
        // Se obtienen los detalles de la reserva para el correo
        var room = await _service.GetRoomDetailsAsync(reservation.RoomId);
        var user = await _service.GetUserDetailsAsync(reservation.UserId);
        
        string reservationDetails = $"Sala: {room.Name}\nUbicación: {room.Location}\nHora de inicio: {reservation.StartTime}\nHora de fin: {reservation.EndTime}";

        // Enviar correo de confirmación
        await _emailService.SendReservationConfirmationAsync(user.Email, reservationDetails);
        

        return new Domain.Entities.Reservations
        {
            UserId = reservation.UserId,
            RoomId = reservation.RoomId,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        };;
    }
}