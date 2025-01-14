using Xunit;
using System.Threading.Tasks;
using Coworking.Infrastructure.Repositories;
using Coworking.Infrastructure.Commands.Users;
using Coworking.Application.Handlers.Users;
using Coworking.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using System;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Users
{
    public class ChangeUserRoleHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldChangeRole_WhenValidRole()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("ChangeRole_Valid")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new UserRepository(context);

            context.Users.Add(new Domain.Entities.Users { Id = 10, Username = "RoleUser", Email = "role@user.com", Password = "hashed", Role = "User" });
            await context.SaveChangesAsync();

            var handler = new ChangeUserRoleHandler(repo);
            var command = new ChangeUserRoleCommand(UserId: 10, NewRole: "Admin");

            bool result = await handler.Handle(command, default);
            Assert.True(result);

            var updated = await context.Users.FindAsync(10);
            Assert.NotNull(updated);
            Assert.Equal("Admin", updated.Role);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenRoleInvalid()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("ChangeRole_Invalid")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new UserRepository(context);

            context.Users.Add(new Domain.Entities.Users { Id = 20, Username = "InvalidRoleUser", Email = "test@test.com" });
            await context.SaveChangesAsync();

            var handler = new ChangeUserRoleHandler(repo);
            var command = new ChangeUserRoleCommand(UserId: 20, NewRole: "SuperAdmin"); // No es "Admin" ni "User"

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserNotFound()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("ChangeRole_UserNotFound")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new UserRepository(context);

            var handler = new ChangeUserRoleHandler(repo);
            var command = new ChangeUserRoleCommand(UserId: 9999, NewRole: "Admin");

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
        }
    }
}
