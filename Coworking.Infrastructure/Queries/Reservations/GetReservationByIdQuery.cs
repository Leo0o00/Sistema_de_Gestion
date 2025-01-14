using MediatR;

namespace Coworking.Infrastructure.Queries.Reservations;

public record GetReservationByIdQuery(int Id) : IRequest<Domain.Entities.Reservations>;
