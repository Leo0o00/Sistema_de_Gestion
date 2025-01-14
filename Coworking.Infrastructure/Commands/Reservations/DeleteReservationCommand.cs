using MediatR;

namespace Coworking.Infrastructure.Commands.Reservations;

public record DeleteReservationCommand(
    int ReservationId,
    int UserId,
    string Role
    ) : IRequest<bool>;