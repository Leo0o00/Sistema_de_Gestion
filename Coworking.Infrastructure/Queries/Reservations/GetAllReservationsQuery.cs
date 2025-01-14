using MediatR;

namespace Coworking.Infrastructure.Queries.Reservations;

public record GetAllReservationsQuery(int UserId, string Role) : IRequest<List<Domain.Entities.Reservations>>;