﻿using MediatR;

namespace Coworking.Infrastructure.Commands.Reservations;

public record CancelReservationCommand(
    int ReservationId,
    int UserId,
    string Role
    ) : IRequest<bool>;