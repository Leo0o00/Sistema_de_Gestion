using MediatR;

namespace Coworking.Infrastructure.Commands.Rooms;

public record RemoveRoomCommand(int Id) : IRequest<bool>;