using MediatR;

namespace Coworking.Infrastructure.Commands.Rooms;

public record CreateRoomCommand(
    string Name,
    string Location,
    int Capacity,
    bool IsActive) : IRequest<Domain.Entities.Rooms>;
