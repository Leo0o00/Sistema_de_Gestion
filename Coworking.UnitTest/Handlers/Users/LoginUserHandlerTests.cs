using Xunit;
using System.Threading.Tasks;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Users;
using Coworking.Infrastructure.Repositories;
using Coworking.Application.Handlers.Users;
using Microsoft.EntityFrameworkCore;
using Coworking.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Users
{
    public class LoginUserHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnToken_WhenCredentialsValid()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("LoginUser_Valid")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new UserRepository(context);

            // Insert user with hashed password "mypassword"
            string hashedPass = BCrypt.Net.BCrypt.HashPassword("mypassword");
            context.Users.Add(new Domain.Entities.Users { Id = 99, Username = "TestUser", Email = "test@user.com", Password = hashedPass, Role = "User" });
            await context.SaveChangesAsync();

            var inMemorySettings = new Dictionary<string, string> { { "Jwt:Key", "TestSecretKeyForLogin" } };
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var handler = new LoginUserHandler(repo, configuration);
            var command = new LoginUserCommand("TestUser", "mypassword");

            var token = await handler.Handle(command, default);
            Assert.NotNull(token);
            Assert.Contains('.', token); // JWT typical structure
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFound()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("LoginUser_NotFound")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new UserRepository(context);

            var inMemorySettings = new Dictionary<string, string> { { "Jwt:Key", "TestSecretKey" } };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var handler = new LoginUserHandler(repo, config);
            var command = new LoginUserCommand("NoUser", "whatever");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorized_WhenPasswordWrong()
        {
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("LoginUser_WrongPassword")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new UserRepository(context);

            var hashedPass = BCrypt.Net.BCrypt.HashPassword("realpassword");
            context.Users.Add(new Domain.Entities.Users { Id = 50, Username = "RealUser", Email = "real@user.com", Password = hashedPass });
            await context.SaveChangesAsync();

            var settings = new Dictionary<string, string> { { "Jwt:Key", "SecretKey" } };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            var handler = new LoginUserHandler(repo, config);
            var command = new LoginUserCommand("RealUser", "wrongpassword");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
        }
    }
}
