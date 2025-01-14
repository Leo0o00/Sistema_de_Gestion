using MediatR;

namespace Coworking.Infrastructure.Commands.Rooms;

public record EditRoomCommand(
    int Id,
    string Name,
    string Location,
    int Capacity,
    bool IsActive) : IRequest<bool>;