using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Tracks;

public class UpdateStudioArtistTrackHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public UpdateStudioArtistTrackHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_UpdateTitleCoverAndCoAuthors_ForCurrentArtistTrack()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var currentUser = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(currentUser);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        var currentArtist = Artist.Create(currentUser.Id, "Owned Artist", null, null, null, VerificationStatus.Verified, utcNow);
        var oldCoAuthor = Artist.Create(null, "Old CoAuthor", null, null, null, VerificationStatus.None, utcNow);
        var newCoAuthor = Artist.Create(null, "New CoAuthor", null, null, null, VerificationStatus.None, utcNow);

        var album = Album.Create(
            "Owned Album",
            null,
            null,
            utcNow.Date,
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            new[] { currentArtist.Id },
            utcNow);

        var track = Track.Create(
            Guid.NewGuid(),
            album.Id,
            1,
            "Old Title",
            180000,
            false,
            "covers/old-cover",
            "tracks/original/source.mp3",
            VisibilityStatus.Published,
            "ISRC12345678",
            new[] { (currentArtist.Id, ArtistRole.Main), (oldCoAuthor.Id, ArtistRole.Feat) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow.AddMinutes(-5));
        track.MarkAsReady("tracks/original/playlist.m3u8", "tracks/original/audio.mp3", utcNow.AddMinutes(-4));

        var coverFileId = Guid.NewGuid();
        dbContext.FileMetadata.Add(FileMetadata.Create(
            coverFileId,
            currentUser.Id,
            "new-cover.jpg",
            "image/jpeg",
            2048,
            $"temp/{coverFileId:N}",
            utcNow));

        dbContext.Artists.AddRange(currentArtist, oldCoAuthor, newCoAuthor);
        dbContext.Albums.Add(album);
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateStudioArtistTrackHandler(
            dbContext,
            _currentUserServiceMock.Object,
            TimeProvider.System);

        await handler.Handle(new UpdateStudioArtistTrackCommand(
            track.Id,
            "New Title",
            coverFileId,
            new List<Guid> { newCoAuthor.Id, currentArtist.Id, newCoAuthor.Id }), CancellationToken.None);

        var updatedTrack = await dbContext.Tracks
            .Include(t => t.TrackArtists)
            .FirstAsync(t => t.Id == track.Id);

        updatedTrack.Title.Should().Be("New Title");
        updatedTrack.CoverUrl.Should().Be($"covers/{coverFileId:N}");
        updatedTrack.AudioStorageKey.Should().Be("tracks/original/audio.mp3");
        updatedTrack.VisibilityStatus.Should().Be(VisibilityStatus.Published);
        updatedTrack.TrackArtists.Should().HaveCount(2);
        updatedTrack.TrackArtists.Should().Contain(x => x.ArtistId == currentArtist.Id && x.Role == ArtistRole.Main);
        updatedTrack.TrackArtists.Should().Contain(x => x.ArtistId == newCoAuthor.Id && x.Role == ArtistRole.Feat);
        updatedTrack.TrackArtists.Should().NotContain(x => x.ArtistId == oldCoAuthor.Id);

        var persistedCover = await dbContext.FileMetadata.FirstAsync(x => x.Id == coverFileId);
        persistedCover.IsTemporary.Should().BeFalse();
        persistedCover.CurrentStorageKey.Should().Be($"covers/{coverFileId:N}");
    }

    [Fact]
    public async Task Handle_Should_ThrowForbidden_WhenTrackAlbumDoesNotBelongToCurrentArtist()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var currentUser = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(currentUser);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        var currentArtist = Artist.Create(currentUser.Id, "Owned Artist", null, null, null, VerificationStatus.None, utcNow);
        var foreignArtist = Artist.Create(null, "Foreign Artist", null, null, null, VerificationStatus.None, utcNow);

        var album = Album.Create(
            "Foreign Album",
            null,
            null,
            utcNow.Date,
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            new[] { foreignArtist.Id },
            utcNow);

        var track = Track.Create(
            Guid.NewGuid(),
            album.Id,
            1,
            "Foreign Track",
            180000,
            false,
            null,
            "tracks/foreign/source.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (foreignArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow);

        dbContext.Artists.AddRange(currentArtist, foreignArtist);
        dbContext.Albums.Add(album);
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateStudioArtistTrackHandler(
            dbContext,
            _currentUserServiceMock.Object,
            TimeProvider.System);

        var action = async () => await handler.Handle(new UpdateStudioArtistTrackCommand(
            track.Id,
            "Updated Title",
            null,
            new List<Guid>()), CancellationToken.None);

        await action.Should().ThrowAsync<ForbiddenException>();
    }
}
