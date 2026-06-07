using MassTransit;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Tests.Infrastructure.Consumers;

public class NotifyUserOnNewFollowerConsumerTests
{
    [Fact]
    public async Task Consume_Should_Notify_TargetUser_When_FollowedByAnotherUser()
    {
        var followerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var msg = new UserFollowedUserEvent(followerId, targetId, "John Doe");

        var consumeContextMock = new Mock<ConsumeContext<UserFollowedUserEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var notificationServiceMock = new Mock<INotificationService>();
        var consumer = new NotifyUserOnNewFollowerConsumer(notificationServiceMock.Object);

        await consumer.Consume(consumeContextMock.Object);

        notificationServiceMock.Verify(x => x.SendToUserAsync(
            targetId,
            NotificationCategory.Social,
            "new_follower",
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("John Doe")),
            NotificationEntityType.User,
            followerId,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_Not_Notify_When_User_Follows_Self()
    {
        var userId = Guid.NewGuid();
        var msg = new UserFollowedUserEvent(userId, userId, "John Doe");

        var consumeContextMock = new Mock<ConsumeContext<UserFollowedUserEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var notificationServiceMock = new Mock<INotificationService>();
        var consumer = new NotifyUserOnNewFollowerConsumer(notificationServiceMock.Object);

        await consumer.Consume(consumeContextMock.Object);

        notificationServiceMock.Verify(x => x.SendToUserAsync(
            It.IsAny<Guid>(),
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
