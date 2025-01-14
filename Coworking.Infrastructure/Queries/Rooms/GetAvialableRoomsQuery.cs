using MediatR;

namespace Coworking.Infrastructure.Queries.Rooms;
public record GetAvailableRoomsQuery(int? Capacity, string? Location) : IRequest<List<Domain.Entities.Rooms>>;


