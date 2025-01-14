using MediatR;

namespace Coworking.Infrastructure.Queries.Rooms;

public record GetRoomByIdQuery(int Id) : IRequest<Domain.Entities.Rooms>;