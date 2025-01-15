using Xunit;
using System;
using System.Threading.Tasks;
using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Repositories;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Application.Handlers.Reservations;
using Microsoft.EntityFrameworkCore;
using Moq;
using Coworking.Infrastructure.Services;
using System.Linq;
using Assert = Xunit.Assert;

namespace Coworking.UnitTest.Handlers.Reservations
{
    public class CreateReservationHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldSendEmail_WhenReservationIsCreated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoworkingDbContext>()
                .UseInMemoryDatabase("CreateReservation_Email")
                .Options;

            await using var context = new CoworkingDbContext(options);
            var repo = new ReservationsRepository(context);

            // Mock del servicio de correo
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock
                .Setup(es => es.SendReservationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var handler = new CreateReservationHandler(repo, emailServiceMock.Object);

            // Sala y usuario base
            context.Rooms.Add(new Domain.Entities.Rooms { Id = 1, Name = "TestRoom", Location = "TestLocation", Capacity = 10, IsActive = true });
            context.Users.Add(new Domain.Entities.Users { Id = 123, Username = "TestUser", Email = "test@user.com", Password = "TestPassword"});
            await context.SaveChangesAsync();

            var command = new CreateReservationCommand(
                UserId: 123,
                RoomId: 1,
                StartTime: DateTime.UtcNow.AddHours(10),
                EndTime: DateTime.UtcNow.AddHours(12)
            );

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            Assert.NotNull(result);
            emailServiceMock.Verify(
                es => es.SendReservationConfirmationAsync("test@user.com", It.IsAny<string>()),
                Times.Once
            );
        }
    }
}