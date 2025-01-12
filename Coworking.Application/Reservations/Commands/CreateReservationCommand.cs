using MediatR;

namespace Coworking.Application.Reservations.Commands;


    public record CreateReservationCommand(
        int UserId,
        int RoomId,
        DateTime StartTime,
        DateTime EndTime
    ) : IRequest<int>;
