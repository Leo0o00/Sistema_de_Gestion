using MediatR;

namespace Coworking.Application.Reservations.Commands;

public record EditReservationCommand(
    int ReservationId,
    DateTime StartTime,
    DateTime EndTime
) : IRequest<bool>;