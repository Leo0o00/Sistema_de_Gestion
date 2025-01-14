﻿using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Application.Handlers.Reservations;
using Microsoft.EntityFrameworkCore;
using Coworking.Infrastructure;
using Coworking.Domain.Entities;
using Coworking.Infrastructure.Commands.Reservations;

namespace Coworking.UnitTests;

    
public class CreateReservationHandlerTests
{
    [Fact]
    public async Task Handle_ShouldThrowException_WhenOverlapExists()
    {
        // Arrange: usar base de datos en memoria
        var options = new DbContextOptionsBuilder<CoworkingDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB_Overlap")
            .Options;

        await using var context = new CoworkingDbContext(options);

        // Crear reserva preexistente
        context.Rooms.Add(new Rooms { Id = 1, Name = "RoomTest", Capacity = 10 });
        context.Reservations.Add(new Reservations
        {
            Id = 100,
            RoomId = 1,
            UserId = 999,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        });
        await context.SaveChangesAsync();

        var handler = new CreateReservationHandler(context);

        var command = new CreateReservationCommand(
            UserId: 1,
            RoomId: 1,
            StartTime: DateTime.UtcNow.AddHours(1),
            EndTime: DateTime.UtcNow.AddHours(3)
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
    }
}

