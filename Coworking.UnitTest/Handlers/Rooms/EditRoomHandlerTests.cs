using Xunit;
using System.Threading.Tasks;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Repositories;
using Coworking.Infrastructure.Commands.Rooms;
using Coworking.Application.Handlers.Rooms;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using Assert = Xunit.Assert;

namespace Coworking.UnitTests.Handlers.Rooms
{
    public class EditRoomHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldEditRoom_WhenRoomExists()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("EditRoom_Exists")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new RoomRepository(context);

            context.Rooms.Add(new Domain.Entities.Rooms { Id = 10, Name = "OldRoom", Location = "OldLoc", Capacity = 10, IsActive = false });
            await context.SaveChangesAsync();

            var handler = new EditRoomHandler(repo);
            var command = new EditRoomCommand(10, "EditedRoom", "EditedLoc", 20, true);

            var result = await handler.Handle(command, default);

            Assert.True(result);
            var updated = await context.Rooms.FindAsync(10);
            Assert.Equal("EditedRoom", updated.Name);
            Assert.Equal("EditedLoc", updated.Location);
            Assert.Equal(20, updated.Capacity);
            Assert.True(updated.IsActive);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenRoomDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("EditRoom_NotFound")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new RoomRepository(context);

            var handler = new EditRoomHandler(repo);
            var command = new EditRoomCommand(999, "Any", "Any", 50, true);

            var result = await handler.Handle(command, default);
            Assert.False(result);
        }
    }
}
