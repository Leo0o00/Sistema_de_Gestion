using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Repositories;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Application.Handlers.Reservations;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using System.Linq;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Reservations
{
    public class EditReservationHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldEditReservation_WhenUserOwnsItAndNoOverlap()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("EditReservation_NoOverlap")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Rooms.Add(new Domain.Entities.Rooms { Id = 1, Name = "EditTest", Location = "EditLocation", Capacity = 10, IsActive = true });
            context.Reservations.Add(new Domain.Entities.Reservations
            {
                Id = 200,
                RoomId = 1,
                UserId = 50,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            });
            await context.SaveChangesAsync();

            var handler = new EditReservationHandler(repo);

            var command = new EditReservationCommand(
                ReservationId: 200,
                StartTime: DateTime.UtcNow.AddHours(2),
                EndTime: DateTime.UtcNow.AddHours(3),
                UserId: 50,  // Dueño de la reserva
                Role: "User" // Rol user
            );

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            Assert.True(result);
            var edited = context.Reservations.Include(r => r.AuditLogs).First(r => r.Id == 200);
            Assert.Equal(DateTime.UtcNow.AddHours(2).Hour, edited.StartTime.Hour);
            Assert.Single(edited.AuditLogs);
            Assert.Equal("Edited", edited.AuditLogs.First().Action);
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorized_WhenUserEditsReservationNotOwned()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("EditReservation_Unauthorized")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Rooms.Add(new Domain.Entities.Rooms { Id = 10, Name = "UnauthorizedRoom", Location = "UnauthorizedLocation", Capacity = 10 });
            context.Reservations.Add(new Domain.Entities.Reservations
            {
                Id = 300,
                RoomId = 10,
                UserId = 99, // Propietario
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            });
            await context.SaveChangesAsync();

            var handler = new EditReservationHandler(repo);
            var command = new EditReservationCommand(
                ReservationId: 300,
                StartTime: DateTime.UtcNow.AddHours(2),
                EndTime: DateTime.UtcNow.AddHours(3),
                UserId: 999,   // Intentando editar, sin ser Admin
                Role: "User"
            );

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperation_WhenOverlapOccurs()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("EditReservation_Overlap")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Rooms.Add(new Domain.Entities.Rooms { Id = 99, Name = "OverlapEditRoom", Location = "OverlapEditLocation", Capacity = 5, IsActive = true });
            // Reserva A
            context.Reservations.Add(new Domain.Entities.Reservations
            {
                Id = 400,
                RoomId = 99,
                UserId = 10,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            });
            // Reserva B (la que se quiere editar)
            context.Reservations.Add(new Domain.Entities.Reservations
            {
                Id = 401,
                RoomId = 99,
                UserId = 10,
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(3)
            });
            await context.SaveChangesAsync();

            var handler = new EditReservationHandler(repo);

            // Editar la B, pero chocando con A
            var command = new EditReservationCommand(
                ReservationId: 401,
                StartTime: DateTime.UtcNow.AddHours(1).AddMinutes(30),
                EndTime: DateTime.UtcNow.AddHours(2).AddMinutes(30),
                UserId: 10,
                Role: "User"
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
        }
    }
}
