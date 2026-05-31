using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;
using Yuviron.Infrastructure.Services;
using Yuviron.Infrastructure.SignalR;

namespace Yuviron.Tests.Infrastructure.Services;

public class NotificationServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public NotificationServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task SendToUserAsync_Should_CreateNotificationInDb_And_SendToSignalR()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var title = "Тестовий заголовок";
        var body = "Текст повідомлення";
        
        var clientProxyMock = new Mock<IYuvironClient>();
        var hubClientsMock = new Mock<IHubClients<IYuvironClient>>();
        
        hubClientsMock.Setup(x => x.Users(It.IsAny<IReadOnlyList<string>>())).Returns(clientProxyMock.Object);
        
        var hubContextMock = new Mock<IHubContext<AppHub, IYuvironClient>>();
        hubContextMock.Setup(x => x.Clients).Returns(hubClientsMock.Object);

        var loggerMock = new Mock<ILogger<NotificationService>>();

        var service = new NotificationService(
            dbContext, 
            TimeProvider.System, 
            hubContextMock.Object, 
            loggerMock.Object);

        await service.SendToUserAsync(userId, title, body, NotificationEntityType.Artist, entityId, CancellationToken.None);

        var notification = await dbContext.Notifications.FirstOrDefaultAsync(n => n.UserId == userId);
        notification.Should().NotBeNull();
        notification!.Title.Should().Be(title);
        notification.Body.Should().Be(body);
        notification.EntityType.Should().Be(NotificationEntityType.Artist);
        notification.EntityId.Should().Be(entityId);
        notification.IsRead.Should().BeFalse();

        clientProxyMock.Verify(x => x.ReceiveNotification(It.Is<NotificationDto>(dto => 
            dto.EntityId == entityId && 
            dto.Title == title &&
            dto.IsRead == false
        )), Times.Once);
    }
}