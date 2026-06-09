using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Playlists.Commands.AddTrackToPlaylist;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Playlists;

public class AddTrackToPlaylistHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly TimeProvider _timeProvider;

    public AddTrackToPlaylistHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _currentUserMock = new Mock<ICurrentUserService>();
        _cacheMock = new Mock<ICacheService>();
        _timeProvider = TimeProvider.System;
    }

    [Fact]
    public async Task Handle_Should_AddTrack_WithCorrectPosition()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var playlist = Playlist.Create(currentUserId, null, "My List", null, null, PlaylistVisibility.Public, false, DateTime.UtcNow);
        dbContext.Playlists.Add(playlist);

        var track1Id = Guid.NewGuid();
        var track2Id = Guid.NewGuid();
        dbContext.Tracks.Add(Track.Create(track1Id, Guid.NewGuid(), 1, "Track 1", 200000, false, null, "key1", VisibilityStatus.Published, null, Array.Empty<(Guid, ArtistRole)>(), Array.Empty<Guid>(), Array.Empty<Guid>(), DateTime.UtcNow));
        dbContext.Tracks.Add(Track.Create(track2Id, Guid.NewGuid(), 2, "Track 2", 200000, false, null, "key2", VisibilityStatus.Published, null, Array.Empty<(Guid, ArtistRole)>(), Array.Empty<Guid>(), Array.Empty<Guid>(), DateTime.UtcNow));
        
        await dbContext.SaveChangesAsync();

        // ИСПРАВЛЕНО: Добавлен _cacheMock.Object
        var handler = new AddTrackToPlaylistHandler(dbContext, _currentUserMock.Object, _timeProvider, _cacheMock.Object);

        await handler.Handle(new AddTrackToPlaylistCommand(playlist.Id, track1Id), CancellationToken.None);
        await handler.Handle(new AddTrackToPlaylistCommand(playlist.Id, track2Id), CancellationToken.None);

        var playlistTracks = await dbContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == playlist.Id)
            .OrderBy(pt => pt.Position)
            .ToListAsync();

        playlistTracks.Should().HaveCount(2);
        playlistTracks[0].TrackId.Should().Be(track1Id);
        playlistTracks[1].TrackId.Should().Be(track2Id);
        playlistTracks[1].Position.Should().BeGreaterThan(playlistTracks[0].Position);
    }

    [Fact]
    public async Task Handle_Should_NotDuplicateTrack_When_AddedTwice()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var playlist = Playlist.Create(currentUserId, null, "My List", null, null, PlaylistVisibility.Public, false, DateTime.UtcNow);
        var trackId = Guid.NewGuid();
        dbContext.Playlists.Add(playlist);
        dbContext.Tracks.Add(Track.Create(trackId, Guid.NewGuid(), 1, "Track 1", 200000, false, null, "key1", VisibilityStatus.Published, null, Array.Empty<(Guid, ArtistRole)>(), Array.Empty<Guid>(), Array.Empty<Guid>(), DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var handler = new AddTrackToPlaylistHandler(dbContext, _currentUserMock.Object, _timeProvider, _cacheMock.Object);

        await handler.Handle(new AddTrackToPlaylistCommand(playlist.Id, trackId), CancellationToken.None);
        await handler.Handle(new AddTrackToPlaylistCommand(playlist.Id, trackId), CancellationToken.None);

        var playlistTracks = await dbContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == playlist.Id)
            .ToListAsync();

        playlistTracks.Should().HaveCount(1);
        playlistTracks[0].TrackId.Should().Be(trackId);
    }
    
    [Fact]
    public async Task Handle_Should_ThrowForbidden_When_AddingToSomeoneElsesPlaylist()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var playlist = Playlist.Create(otherUserId, null, "Other List", null, null, PlaylistVisibility.Public, false, DateTime.UtcNow);
        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync();

        // ИСПРАВЛЕНО: Добавлен _cacheMock.Object
        var handler = new AddTrackToPlaylistHandler(dbContext, _currentUserMock.Object, _timeProvider, _cacheMock.Object);

        Func<Task> action = async () => await handler.Handle(new AddTrackToPlaylistCommand(playlist.Id, Guid.NewGuid()), CancellationToken.None);
        await action.Should().ThrowAsync<ForbiddenException>();
    }
}
