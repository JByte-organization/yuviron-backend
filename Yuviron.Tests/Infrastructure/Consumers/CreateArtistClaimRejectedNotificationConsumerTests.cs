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

public class CreateArtistClaimRejectedNotificationConsumerTests
{
    [Fact]
    public async Task Consume_Should_CallNotificationService_WithCorrectParameters_WithAdminNote()
    {
        var userId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var artistName = "Test Artist";
        var adminNote = "Invalid links provided";
        var eventMessage = new ArtistClaimRejectedEvent(userId, artistId, artistName, adminNote);

        var consumeContextMock = new Mock<ConsumeContext<ArtistClaimRejectedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(eventMessage);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var notificationServiceMock = new Mock<INotificationService>();
        var consumer = new CreateArtistClaimRejectedNotificationConsumer(notificationServiceMock.Object);

        await consumer.Consume(consumeContextMock.Object);

        notificationServiceMock.Verify(x => x.SendToUserAsync(
            userId,
            NotificationCategory.System,
            "artist_claim_rejected",
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains(adminNote)),
            NotificationEntityType.Artist,
            artistId,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
