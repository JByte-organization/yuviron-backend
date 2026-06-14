using Yuviron.Infrastructure.Consumers.Notifications.Email;
using Yuviron.Infrastructure.Consumers.Notifications.InApp;
﻿using MassTransit;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers;

namespace Yuviron.Tests.Infrastructure.Consumers;

public class NotifyUserOnTeamRoleChangedConsumerTests
{
    [Fact]
    public async Task Consume_Should_CallNotificationService_WithCorrectParameters()
    {
        var userId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var artistName = "Test Artist";
        var eventMessage = new TeamRoleChangedEvent(userId, artistId, artistName, ArtistTeamRole.Manager);

        var consumeContextMock = new Mock<ConsumeContext<TeamRoleChangedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(eventMessage);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var notificationServiceMock = new Mock<INotificationService>();
        var consumer = new NotifyUserOnTeamRoleChangedConsumer(notificationServiceMock.Object);

        await consumer.Consume(consumeContextMock.Object);

        notificationServiceMock.Verify(x => x.SendToUserAsync(
            userId,
            NotificationCategory.System,
            "team_role_changed",
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains(artistName)),
            NotificationEntityType.Artist,
            artistId,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
