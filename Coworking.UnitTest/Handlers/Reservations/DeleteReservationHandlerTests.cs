using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Repositories;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Application.Handlers.Reservations;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Reservations
{
    public class DeleteReservationHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldDeleteReservation_WhenUserIsAdmin()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("DeleteReservation_Admin")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Reservations.Add(new Domain.Entities.Reservations
            {
                Id = 600,
                RoomId = 1,
                UserId = 100
            });
            await context.SaveChangesAsync();

            var handler = new DeleteReservationHandler(repo);
            var command = new DeleteReservationCommand(
                ReservationId: 600,
                UserId: 1, // el Admin
                Role: "Admin"
            );

            // Act
            bool result = await handler.Handle(command, default);

            // Assert
            Assert.True(result);
            Assert.Null(await context.Reservations.FirstOrDefaultAsync(r => r.Id == 600));
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotAdmin()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("DeleteReservation_NonAdmin")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Reservations.Add(new Domain.Entities.Reservations { Id = 601, RoomId = 10, UserId = 99 });
            await context.SaveChangesAsync();

            var handler = new DeleteReservationHandler(repo);
            var command = new DeleteReservationCommand(
                ReservationId: 601,
                UserId: 999,
                Role: "User"
            );

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
        }
    }
}
