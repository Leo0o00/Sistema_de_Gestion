using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Repositories;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Application.Handlers.Reservations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Reservations
{
    public class CreateReservationHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCreateReservation_WhenNoOverlap()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("CreateReservation_NoOverlap")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            // Sala y usuario base
            context.Rooms.Add(new Domain.Entities.Rooms { Id = 1, Name = "TestRoom", Location = "TestLocation", Capacity = 10, IsActive = true });
            await context.SaveChangesAsync();

            var handler = new CreateReservationHandler(repo);
            var command = new CreateReservationCommand(
                UserId: 123,
                RoomId: 1,
                StartTime: DateTime.UtcNow.AddHours(10),
                EndTime: DateTime.UtcNow.AddHours(12)
            );

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.RoomId);
            Assert.Equal(123, result.UserId);

            var reservation = context.Reservations.Include(r => r.AuditLogs).FirstOrDefault(r => r.Id == result.Id);
            Assert.NotNull(reservation);
            Assert.Single(reservation!.AuditLogs);
            Assert.Equal("Created", reservation.AuditLogs.First().Action);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenOverlapExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("CreateReservation_Overlap")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Rooms.Add(new Domain.Entities.Rooms { Id = 1, Name = "OverlapRoom", Location = "OverlapLocation", Capacity = 5, IsActive = true });
            context.Reservations.Add(new Domain.Entities.Reservations
            {
                Id = 100,
                RoomId = 1,
                UserId = 999,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(3)
            });
            await context.SaveChangesAsync();

            var handler = new CreateReservationHandler(repo);
            var command = new CreateReservationCommand(
                UserId: 123,
                RoomId: 1,
                StartTime: DateTime.UtcNow.AddHours(2),
                EndTime: DateTime.UtcNow.AddHours(4)
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
        }
    }
}
