using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Playlists.Commands.ChangeTrackPosition;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Playlists;

public class ChangeTrackPositionHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly TimeProvider _timeProvider;

    public ChangeTrackPositionHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _cacheMock = new Mock<ICacheService>();
        _timeProvider = TimeProvider.System;
    }

    [Fact]
    public async Task Handle_Should_CalculateNewPosition_Correctly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new AppDbContext(options);

        var currentUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var playlist = Playlist.Create(currentUserId, null, "Mix", null, null, PlaylistVisibility.Public, false, DateTime.UtcNow);
        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync();

        var track3Id = Guid.NewGuid();
        dbContext.Tracks.Add(Track.Create(track3Id, Guid.NewGuid(), 1, "Track 3", 1000, false, null, "key", VisibilityStatus.Published, null, [], [], [], DateTime.UtcNow));
        dbContext.PlaylistTracks.Add(new PlaylistTrack(playlist.Id, Guid.NewGuid(), 1000, currentUserId, DateTime.UtcNow));
        dbContext.PlaylistTracks.Add(new PlaylistTrack(playlist.Id, Guid.NewGuid(), 2000, currentUserId, DateTime.UtcNow));
        dbContext.PlaylistTracks.Add(new PlaylistTrack(playlist.Id, track3Id, 3000, currentUserId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var handler = new ChangeTrackPositionHandler(dbContext, _currentUserMock.Object, _timeProvider, _cacheMock.Object);

        // Act
        await handler.Handle(new ChangeTrackPositionCommand(playlist.Id, track3Id, 1500), CancellationToken.None);

        // Assert
        var pt = await dbContext.PlaylistTracks.FirstOrDefaultAsync(x => x.TrackId == track3Id);
        pt.Should().NotBeNull();
        pt!.Position.Should().Be(1500);
    }
}

