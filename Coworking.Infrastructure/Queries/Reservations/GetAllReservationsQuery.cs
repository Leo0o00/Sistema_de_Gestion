using MediatR;

namespace Coworking.Infrastructure.Queries.Reservations;

public record GetAllReservationsQuery : IRequest<IEnumerable<Domain.Entities.Reservations>>;