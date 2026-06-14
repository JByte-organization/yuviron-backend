using Yuviron.Infrastructure.Consumers.Notifications.Email;
using Yuviron.Infrastructure.Consumers.Notifications.InApp;
﻿using MassTransit;
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

public class NotifyUserOnPlaylistFavoritedConsumerTests
{
    [Fact]
    public async Task Consume_Should_Notify_Owner_When_Playlist_Saved_By_Another_User()
    {
        var ownerId = Guid.NewGuid();
        var savedById = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var msg = new PlaylistAddedToFavoritesEvent(playlistId, "Chill Vibes", ownerId, savedById, "Jane Doe");

        var consumeContextMock = new Mock<ConsumeContext<PlaylistAddedToFavoritesEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var notificationServiceMock = new Mock<INotificationService>();
        var consumer = new NotifyUserOnPlaylistFavoritedConsumer(notificationServiceMock.Object);

        await consumer.Consume(consumeContextMock.Object);

        notificationServiceMock.Verify(x => x.SendToUserAsync(
            ownerId,
            NotificationCategory.Social,
            "playlist_favorited",
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("Jane Doe") && s.Contains("Chill Vibes")),
            NotificationEntityType.Playlist,
            playlistId,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_Not_Notify_Owner_When_Owner_Saves_Own_Playlist()
    {
        var ownerId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var msg = new PlaylistAddedToFavoritesEvent(playlistId, "Chill Vibes", ownerId, ownerId, "Jane Doe");

        var consumeContextMock = new Mock<ConsumeContext<PlaylistAddedToFavoritesEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var notificationServiceMock = new Mock<INotificationService>();
        var consumer = new NotifyUserOnPlaylistFavoritedConsumer(notificationServiceMock.Object);

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
