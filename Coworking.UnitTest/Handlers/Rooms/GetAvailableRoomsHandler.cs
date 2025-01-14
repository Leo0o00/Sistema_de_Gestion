using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Application.Handlers.Rooms;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Queries.Rooms;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using System.Linq;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Rooms
{
    public class GetAvailableRoomsHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnActiveRooms_WithCapacityAndLocationFiltering()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("GetAvailableRoomsFilter")
                .Options;

            await using var context = new CoworkingDbContext(options);
            
            context.Rooms.Add(new Domain.Entities.Rooms { Id = 1, Name = "RoomA", Location = "Office1", Capacity = 5, IsActive = true });
            context.Rooms.Add(new Domain.Entities.Rooms { Id = 2, Name = "RoomB", Location = "Office2", Capacity = 10, IsActive = true });
            context.Rooms.Add(new Domain.Entities.Rooms { Id = 3, Name = "RoomC", Location = "Office2", Capacity = 2, IsActive = false });
            await context.SaveChangesAsync();

            var handler = new GetAvailableRoomsHandler(context);
            var query = new GetAvailableRoomsQuery(Capacity: 6, Location: "Office2");

            var result = await handler.Handle(query, default);

            // Must only return RoomB (capacity=10, location=Office2, isActive=true)
            Assert.Single(result);
            Assert.Equal("RoomB", result[0].Name);
        }
    }
}