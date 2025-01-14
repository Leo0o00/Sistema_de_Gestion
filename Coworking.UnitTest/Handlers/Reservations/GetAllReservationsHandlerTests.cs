using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Application.Handlers.Reservations;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Queries.Reservations;
using Coworking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Reservations
{
    public class GetAllReservationsHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnAll_WhenAdmin()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("GetAllReservations_Admin")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Reservations.AddRange(
                new Domain.Entities.Reservations { Id = 1, UserId = 10 },
                new Domain.Entities.Reservations { Id = 2, UserId = 11 }
            );
            await context.SaveChangesAsync();

            var handler = new GetAllReservationsHandler(repo);
            var query = new GetAllReservationsQuery(UserId: 999, Role: "Admin");

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Handle_ShouldReturnOnlyOwned_WhenUser()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("GetAllReservations_User")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            context.Reservations.AddRange(
                new Domain.Entities.Reservations { Id = 10, UserId = 50 },
                new Domain.Entities.Reservations { Id = 11, UserId = 60 }
            );
            await context.SaveChangesAsync();

            var handler = new GetAllReservationsHandler(repo);
            var query = new GetAllReservationsQuery(UserId: 50, Role: "User");

            var result = await handler.Handle(query, default);

            Assert.Single(result);
            Assert.Equal(10, result[0].Id);
        }
    }
}
