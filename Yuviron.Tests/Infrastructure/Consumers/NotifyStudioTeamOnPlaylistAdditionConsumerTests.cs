using Yuviron.Infrastructure.Consumers.Notifications.Email;
using Yuviron.Infrastructure.Consumers.Notifications.InApp;
﻿using MassTransit;
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

public class NotifyStudioTeamOnPlaylistAdditionConsumerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly NotifyStudioTeamOnPlaylistAdditionConsumer _consumer;

    public NotifyStudioTeamOnPlaylistAdditionConsumerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new AppDbContext(options);
        _notificationServiceMock = new Mock<INotificationService>();
        _consumer = new NotifyStudioTeamOnPlaylistAdditionConsumer(_context, _notificationServiceMock.Object);
    }

    [Fact]
    public async Task Consume_Should_Notify_All_Team_Members()
    {
        // Arrange
        var artist = Artist.Create(null, "Test Artist", null, null, null, VerificationStatus.None, DateTime.UtcNow); _context.Add(artist);
        var artistId = artist.Id;

        var member1 = User.Create("member1@test.com", "hash", "M1", true, true, DateTime.UtcNow);
        var member2 = User.Create("member2@test.com", "hash", "M2", true, true, DateTime.UtcNow); _context.AddRange(member1, member2); _context.Add(ArtistTeamMember.Create(artistId, member1.Id, ArtistTeamRole.Owner, DateTime.UtcNow)); _context.Add(ArtistTeamMember.Create(artistId, member2.Id, ArtistTeamRole.Manager, DateTime.UtcNow));
        await _context.SaveChangesAsync();

        var trackId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var msg = new TrackAddedToEditorialPlaylistEvent(artistId, trackId, "Super Hit", "Top 50");
        
        var consumeContextMock = new Mock<ConsumeContext<TrackAddedToEditorialPlaylistEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _notificationServiceMock.Verify(x => x.SendToUsersAsync(
            It.Is<IEnumerable<Guid>>(ids => ids.Contains(member1.Id) && ids.Contains(member2.Id) && ids.Count() == 2),
            NotificationCategory.Music,
            "editorial_playlist",
            It.IsAny<string>(),
            It.IsAny<string>(),
            NotificationEntityType.Track,
            trackId,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}


