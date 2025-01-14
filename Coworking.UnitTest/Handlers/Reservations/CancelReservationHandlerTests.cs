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
    public class CancelReservationHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCancelReservation_WhenUserIsOwner()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("CancelReservation_Owner")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Reservations.Add(new Domain.Entities.Reservations
            {
                Id = 500,
                RoomId = 1,
                UserId = 44,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                IsCancelled = false
            });
            await context.SaveChangesAsync();

            var handler = new CancelReservationHandler(repo);
            var command = new CancelReservationCommand(
                ReservationId: 500,
                UserId: 44,
                Role: "User"
            );

            // Act
            bool result = await handler.Handle(command, default);

            // Assert
            Assert.True(result);
            var res = context.Reservations.Include(r => r.AuditLogs).First(r => r.Id == 500);
            Assert.True(res.IsCancelled);
            Assert.Single(res.AuditLogs);
            Assert.Equal("Cancelled", res.AuditLogs.First().Action);
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorized_WhenUserNotOwner()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("CancelReservation_Unauthorized")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Reservations.Add(new Domain.Entities.Reservations
            {
                Id = 501,
                RoomId = 1,
                UserId = 44
            });
            await context.SaveChangesAsync();

            var handler = new CancelReservationHandler(repo);
            var command = new CancelReservationCommand(
                ReservationId: 501,
                UserId: 999,  // no es el propietario
                Role: "User"
            );

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
        }
    }
}
