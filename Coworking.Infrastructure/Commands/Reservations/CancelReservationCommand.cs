using MediatR;

namespace Coworking.Infrastructure.Commands.Reservations;

public record CancelReservationCommand(int ReservationId) : IRequest<bool>;