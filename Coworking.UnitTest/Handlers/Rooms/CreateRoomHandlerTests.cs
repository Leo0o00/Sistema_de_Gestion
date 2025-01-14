using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Repositories;
using Coworking.Infrastructure.Commands.Rooms;
using Coworking.Application.Handlers.Rooms;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Rooms
{
    public class CreateRoomHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCreateRoom_WhenNameIsNotUsed()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("CreateRoom_NoDuplicate")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new RoomRepository(context);
            var handler = new CreateRoomHandler(repo);

            var command = new CreateRoomCommand("NewRoom", "LocationX", 20, true);

            var room = await handler.Handle(command, default);

            Assert.NotNull(room);
            Assert.Equal("NewRoom", room.Name);
            Assert.Equal("LocationX", room.Location);

            var inDb = context.Rooms.FirstOrDefault(r => r.Name == "NewRoom");
            Assert.NotNull(inDb);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenNameAlreadyUsed()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("CreateRoom_Duplicate")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new RoomRepository(context);

            context.Rooms.Add(new Domain.Entities.Rooms { Id = 1, Name = "DuplicateName", Location = "Loc", Capacity = 10, IsActive = true });
            await context.SaveChangesAsync();

            var handler = new CreateRoomHandler(repo);
            var command = new CreateRoomCommand("DuplicateName", "AnotherLoc", 5, true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
        }
    }
}
