using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Repositories;
using Coworking.Infrastructure.Commands.Users;
using Coworking.Application.Handlers.Users;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Users
{
    public class RegisterUserHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldRegisterUser_WhenUsernameAndEmailUnique()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("RegisterUser_Unique")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new UserRepository(context);

            var handler = new RegisterUserHandler(repo);
            var command = new RegisterUserCommand("NewUser", "newuser@test.com", "1234");

            int newUserId = await handler.Handle(command, default);
            Assert.True(newUserId > 0);

            var created = await context.Users.FindAsync(newUserId);
            Assert.NotNull(created);
            Assert.Equal("User", created.Role);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenUsernameExists()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("RegisterUser_DuplicateUsername")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new UserRepository(context);

            context.Users.Add(new Domain.Entities.Users { Username = "Duplicate", Email = "dup@test.com", Password = "hash" });
            await context.SaveChangesAsync();

            var handler = new RegisterUserHandler(repo);
            var command = new RegisterUserCommand("Duplicate", "xx@test.com", "1234");

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenEmailExists()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("RegisterUser_DuplicateEmail")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new UserRepository(context);

            context.Users.Add(new Domain.Entities.Users { Username = "TestUser", Email = "same@test.com", Password = "hash" });
            await context.SaveChangesAsync();

            var handler = new RegisterUserHandler(repo);
            var command = new RegisterUserCommand("NewUsername", "same@test.com", "1234");

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
        }
    }
}
