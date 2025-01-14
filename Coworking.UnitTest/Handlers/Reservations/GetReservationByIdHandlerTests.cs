using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Application.Handlers.Reservations;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Queries.Reservations;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Reservations
{
    public class GetReservationByIdHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnReservation_WhenFound()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("GetReservationById_Found")
                .Options;

            await using var context = new CoworkingDbContext(options);
            context.Rooms.Add(new Domain.Entities.Rooms { Id = 1, Name = "RoomTest", Location = "LocationTest", Capacity = 14, IsActive = true});
            context.Users.Add(new Domain.Entities.Users { Id = 2, Username = "UserTest", Email = "test@test.com" });
            context.Reservations.Add(new Domain.Entities.Reservations
            {
                Id = 700,
                RoomId = 1,
                UserId = 2,
                StartTime = DateTime.Now
            });
            await context.SaveChangesAsync();

            var handler = new GetReservationByIdHandler(context);
            var query = new GetReservationByIdQuery(700);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(700, result.Id);
            Assert.Equal("RoomTest", result.Room.Name);
            Assert.Equal("UserTest", result.User.Username);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenNotFound()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("GetReservationById_NotFound")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var handler = new GetReservationByIdHandler(context);

            var query = new GetReservationByIdQuery(999);

            await Assert.ThrowsAsync<Exception>(() => handler.Handle(query, default));
        }
    }
}
