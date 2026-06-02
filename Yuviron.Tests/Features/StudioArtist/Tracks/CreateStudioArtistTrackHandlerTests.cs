using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Tracks;

public class CreateStudioArtistTrackHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IAudioMetadataService> _audioMetadataServiceMock;

    public CreateStudioArtistTrackHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _audioMetadataServiceMock = new Mock<IAudioMetadataService>();
    }

    [Fact]
    public async Task Handle_Should_CreateDraftTrack_ForCurrentArtistAlbum()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var currentUser = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(currentUser);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        var currentArtist = Artist.Create(currentUser.Id, "Owned Artist", null, null, null, VerificationStatus.Verified, utcNow);
        var coAuthor = Artist.Create(null, "Featured Artist", null, null, null, VerificationStatus.None, utcNow);

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

        var existingTrack = Track.Create(
            Guid.NewGuid(),
            album.Id,
            1,
            "Existing Track",
            180000,
            false,
            null,
            "tracks/existing/source.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (currentArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow.AddMinutes(-5));

        var audioFileId = Guid.NewGuid();
        var coverFileId = Guid.NewGuid();

        dbContext.FileMetadata.Add(FileMetadata.Create(
            audioFileId,
            currentUser.Id,
            "song.mp3",
            "audio/mpeg",
            1024,
            $"temp/{audioFileId:N}",
            utcNow));
        dbContext.FileMetadata.Add(FileMetadata.Create(
            coverFileId,
            currentUser.Id,
            "cover.jpg",
            "image/jpeg",
            2048,
            $"temp/{coverFileId:N}",
            utcNow));

        dbContext.Artists.AddRange(currentArtist, coAuthor);
        dbContext.Albums.Add(album);
        dbContext.Tracks.Add(existingTrack);

        _audioMetadataServiceMock
            .Setup(x => x.GetAudioMetadataAsync($"temp/{audioFileId:N}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AudioMetadata(215000, "audio/mpeg", 320000, "mp3"));

        await dbContext.SaveChangesAsync();

        var handler = new CreateStudioArtistTrackHandler(
            dbContext,
            TimeProvider.System,
            _audioMetadataServiceMock.Object,
            _currentUserServiceMock.Object);

        var result = await handler.Handle(new CreateStudioArtistTrackCommand(
            album.Id,
            "New Single",
            true,
            audioFileId,
            coverFileId,
            new List<Guid> { coAuthor.Id, coAuthor.Id, currentArtist.Id }), CancellationToken.None);

        var createdTrack = await dbContext.Tracks
            .Include(t => t.TrackArtists)
            .FirstAsync(t => t.Id == result);

        createdTrack.AlbumId.Should().Be(album.Id);
        createdTrack.AlbumPosition.Should().Be(2);
        createdTrack.Title.Should().Be("New Single");
        createdTrack.Explicit.Should().BeTrue();
        createdTrack.VisibilityStatus.Should().Be(VisibilityStatus.Draft);
        createdTrack.AudioStorageKey.Should().Be($"temp/{audioFileId:N}");
        createdTrack.CoverUrl.Should().Be($"covers/{coverFileId:N}");
        createdTrack.TrackArtists.Should().HaveCount(2);
        createdTrack.TrackArtists.Should().Contain(x => x.ArtistId == currentArtist.Id && x.Role == ArtistRole.Main);
        createdTrack.TrackArtists.Should().Contain(x => x.ArtistId == coAuthor.Id && x.Role == ArtistRole.Feat);

        var persistedAudio = await dbContext.FileMetadata.FirstAsync(x => x.Id == audioFileId);
        var persistedCover = await dbContext.FileMetadata.FirstAsync(x => x.Id == coverFileId);

        persistedAudio.IsTemporary.Should().BeFalse();
        persistedAudio.CurrentStorageKey.Should().StartWith($"tracks/{result}/");
        persistedCover.IsTemporary.Should().BeFalse();
        persistedCover.CurrentStorageKey.Should().Be($"covers/{coverFileId:N}");
    }

    [Fact]
    public async Task Handle_Should_ThrowForbidden_WhenAlbumDoesNotBelongToCurrentArtist()
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

        var audioFileId = Guid.NewGuid();
        dbContext.FileMetadata.Add(FileMetadata.Create(
            audioFileId,
            currentUser.Id,
            "song.mp3",
            "audio/mpeg",
            1024,
            $"temp/{audioFileId:N}",
            utcNow));

        dbContext.Artists.AddRange(currentArtist, foreignArtist);
        dbContext.Albums.Add(album);
        await dbContext.SaveChangesAsync();

        var handler = new CreateStudioArtistTrackHandler(
            dbContext,
            TimeProvider.System,
            _audioMetadataServiceMock.Object,
            _currentUserServiceMock.Object);

        var action = async () => await handler.Handle(new CreateStudioArtistTrackCommand(
            album.Id,
            "Forbidden Track",
            false,
            audioFileId,
            null,
            null), CancellationToken.None);

        await action.Should().ThrowAsync<ForbiddenException>();
    }
}
