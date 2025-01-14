using MediatR;

namespace Coworking.Infrastructure.Commands.Reservations;


    public record CreateReservationCommand(
        int UserId,
        int RoomId,
        DateTime StartTime,
        DateTime EndTime
    ) : IRequest<Domain.Entities.Reservations>;
