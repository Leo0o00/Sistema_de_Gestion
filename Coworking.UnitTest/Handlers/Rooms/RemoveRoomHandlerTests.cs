using Xunit;
using System.Threading.Tasks;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Rooms;
using Coworking.Infrastructure.Repositories;
using Coworking.Application.Handlers.Rooms;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using Assert = Xunit.Assert;

namespace Coworking.UnitTests.Handlers.Rooms
{
    public class RemoveRoomHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldRemoveRoom_WhenItExists()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("RemoveRoom_Exists")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new RoomRepository(context);

            context.Rooms.Add(new Domain.Entities.Rooms { Id = 100, Name = "ToRemove", Location = "Loc", Capacity = 10, IsActive = true });
            await context.SaveChangesAsync();

            var handler = new RemoveRoomHandler(repo);
            var command = new RemoveRoomCommand(100);

            var result = await handler.Handle(command, default);
            Assert.True(result);

            var inDb = await context.Rooms.FindAsync(100);
            Assert.Null(inDb);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenRoomNotExist()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("RemoveRoom_NotExist")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new RoomRepository(context);

            var handler = new RemoveRoomHandler(repo);
            var command = new RemoveRoomCommand(200);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
        }
    }
}