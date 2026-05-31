using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Notifications.Commands.MarkAsRead;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Notifications;

public class MarkNotificationAsReadHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public MarkNotificationAsReadHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_MarkAsRead_When_UserOwnsNotification()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        var notification = Notification.Create(currentUserId, "Title", "Body", null, null, DateTime.UtcNow);
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        var handler = new MarkNotificationAsReadHandler(dbContext, _currentUserServiceMock.Object);
        var command = new MarkNotificationAsReadCommand(notification.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedNotif = await dbContext.Notifications.FindAsync(notification.Id);
        updatedNotif!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_When_NotificationBelongsToOtherUser()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid(); // Хакер
        var victimUserId = Guid.NewGuid();  // Жертва
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        var notification = Notification.Create(victimUserId, "Secret Info", "Body", null, null, DateTime.UtcNow);
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        var handler = new MarkNotificationAsReadHandler(dbContext, _currentUserServiceMock.Object);
        var command = new MarkNotificationAsReadCommand(notification.Id);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);

        // Добавили экранированные кавычки вокруг \"Notification\"
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*\"Notification\" ({notification.Id}) was not found*");
    }
}