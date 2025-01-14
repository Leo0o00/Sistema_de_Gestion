using MediatR;

namespace Coworking.Infrastructure.Queries.Rooms;

public record GetAllRoomsQuery : IRequest<List<Domain.Entities.Rooms>>;
