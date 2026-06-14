using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Infrastructure.Consumers;

public class NotifyFollowersOnNewReleaseConsumerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly NotifyFollowersOnNewReleaseConsumer _consumer;

    public NotifyFollowersOnNewReleaseConsumerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new AppDbContext(options);
        _notificationServiceMock = new Mock<INotificationService>();
        _consumer = new NotifyFollowersOnNewReleaseConsumer(_context, _notificationServiceMock.Object);
    }

    [Fact]
    public async Task Consume_Should_Notify_Followers_With_Notifications_Enabled()
    {
        // Arrange
        var artistId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid(); _context.Add(new UserFollowArtist(user1Id, artistId, true, DateTime.UtcNow)); _context.Add(new UserFollowArtist(user2Id, artistId, true, DateTime.UtcNow));
        await _context.SaveChangesAsync();

        var msg = new NewReleasePublishedEvent(artistId, "The Beatles", Guid.NewGuid(), NotificationEntityType.Album, "Abbey Road", null);
        var consumeContextMock = new Mock<ConsumeContext<NewReleasePublishedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _notificationServiceMock.Verify(x => x.SendToUsersAsync(
            It.Is<IEnumerable<Guid>>(ids => ids.Contains(user1Id) && ids.Contains(user2Id) && ids.Count() == 2),
            It.IsAny<NotificationCategory>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationEntityType>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_Not_Notify_Followers_With_Notifications_Disabled()
    {
        // Arrange
        var artistId = Guid.NewGuid();
        var user1Id = Guid.NewGuid(); 
        var user2Id = Guid.NewGuid(); _context.Add(new UserFollowArtist(user1Id, artistId, true, DateTime.UtcNow)); _context.Add(new UserFollowArtist(user2Id, artistId, false, DateTime.UtcNow));
        await _context.SaveChangesAsync();

        var msg = new NewReleasePublishedEvent(artistId, "The Beatles", Guid.NewGuid(), NotificationEntityType.Album, "Abbey Road", null);
        var consumeContextMock = new Mock<ConsumeContext<NewReleasePublishedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _notificationServiceMock.Verify(x => x.SendToUsersAsync(
            It.Is<IEnumerable<Guid>>(ids => ids.Contains(user1Id) && !ids.Contains(user2Id) && ids.Count() == 1),
            It.IsAny<NotificationCategory>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationEntityType>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_Not_Call_NotificationService_When_No_Followers()
    {
        // Arrange
        var artistId = Guid.NewGuid(); 
        
        var msg = new NewReleasePublishedEvent(artistId, "The Beatles", Guid.NewGuid(), NotificationEntityType.Album, "Abbey Road", null);
        var consumeContextMock = new Mock<ConsumeContext<NewReleasePublishedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _notificationServiceMock.Verify(x => x.SendToUsersAsync(
            It.IsAny<IEnumerable<Guid>>(),
            It.IsAny<NotificationCategory>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationEntityType>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }
}
