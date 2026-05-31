using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Notifications.Queries.GetUnreadCount;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Notifications;

public class GetUnreadNotificationCountHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnCorrectUnreadCount()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new AppDbContext(dbOptions);
        
        var currentUserId = Guid.NewGuid();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        var utcNow = DateTime.UtcNow;

        // 1. Непрочитанное
        dbContext.Notifications.Add(Notification.Create(currentUserId, "Title 1", "Body", null, null, utcNow));
        
        // 2. Еще одно непрочитанное
        dbContext.Notifications.Add(Notification.Create(currentUserId, "Title 2", "Body", null, null, utcNow));
        
        // 3. ПРОЧИТАННОЕ (не должно считаться)
        var readNotif = Notification.Create(currentUserId, "Title 3", "Body", null, null, utcNow);
        readNotif.MarkAsRead();
        dbContext.Notifications.Add(readNotif);

        // 4. Непрочитанное, но ЧУЖОЕ (не должно считаться)
        dbContext.Notifications.Add(Notification.Create(Guid.NewGuid(), "Title 4", "Body", null, null, utcNow));

        await dbContext.SaveChangesAsync();

        var handler = new GetUnreadNotificationCountHandler(dbContext, currentUserServiceMock.Object);
        
        // Act
        var result = await handler.Handle(new GetUnreadNotificationCountQuery(), CancellationToken.None);

        // Assert
        result.Should().Be(2); // Ровно два непрочитанных уведомления для этого юзера
    }
}