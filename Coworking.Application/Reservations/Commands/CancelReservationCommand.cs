using MediatR;

namespace Coworking.Application.Reservations.Commands;

public record CancelReservationCommand(int ReservationId) : IRequest<bool>;