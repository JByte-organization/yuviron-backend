using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Notifications.Commands.MarkAsRead;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.SocialAndNotifications;

public class MarkNotificationAsReadHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public MarkNotificationAsReadHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_ThrowForbidden_When_NotificationBelongsToOtherUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new AppDbContext(options);

        var ownerId = Guid.NewGuid();
        var hackerId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(hackerId);

        var notification = Notification.Create(
            ownerId, 
            (NotificationCategory)1, 
            "System", 
            "Секрет", 
            "Инфо", 
            null, null, DateTime.UtcNow);
        
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        var handler = new MarkNotificationAsReadHandler(dbContext, _currentUserMock.Object);

        // Act & Assert
        // Ожидаем именно Forbidden, так как владелец userId не совпадает с тем, кто запрашивает
        await Assert.ThrowsAsync<ForbiddenException>(async () => 
            await handler.Handle(new MarkNotificationAsReadCommand(notification.Id), CancellationToken.None));
    }
}