using MediatR;

namespace Coworking.Infrastructure.Commands.Reservations;

public record EditReservationCommand(
    int ReservationId,
    DateTime StartTime,
    DateTime EndTime
) : IRequest<bool>;