using MediatR;

namespace Coworking.Application.Rooms.Queries;
public record GetAvailableRoomsQuery(int? Capacity, string? Location) : IRequest<List<Domain.Entities.Rooms>>;


