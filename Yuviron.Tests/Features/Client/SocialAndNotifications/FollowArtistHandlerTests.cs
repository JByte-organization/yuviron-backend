using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Artists.Commands.FollowArtist;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.SocialAndNotifications;

public class FollowArtistHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly TimeProvider _timeProvider;

    public FollowArtistHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _eventBusMock = new Mock<IEventBus>();
        _timeProvider = TimeProvider.System;
    }

    [Fact]
    public async Task Handle_Should_FollowArtist_And_PublishEvent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new AppDbContext(options);

        var currentUserId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var artist = Artist.Create(artistId, "The Weekend", null, null, null, VerificationStatus.Verified, _timeProvider.GetUtcNow().UtcDateTime);
        dbContext.Artists.Add(artist);
        await dbContext.SaveChangesAsync();

        var handler = new FollowArtistHandler(dbContext, _currentUserMock.Object, _eventBusMock.Object, _timeProvider);
    
        // Act
        await handler.Handle(new FollowArtistCommand(artist.Id), CancellationToken.None);

        // Assert
        var followRecord = await dbContext.UserFollowArtists.FirstOrDefaultAsync(x => x.UserId == currentUserId && x.ArtistId == artist.Id);
        followRecord.Should().NotBeNull();
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<UserFollowedArtistEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
