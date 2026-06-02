using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Tracks;

public class DeleteStudioArtistTrackHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public DeleteStudioArtistTrackHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_SoftDeleteTrack_ForCurrentArtist()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var user = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(user);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        var artist = Artist.Create(user.Id, "Owned Artist", null, null, null, VerificationStatus.Verified, utcNow);
        var album = Album.Create(
            "Owned Album",
            null,
            null,
            utcNow.Date,
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            new[] { artist.Id },
            utcNow);

        var track = Track.Create(
            Guid.NewGuid(),
            album.Id,
            1,
            "Owned Track",
            180000,
            false,
            "covers/owned-cover.jpg",
            "tracks/owned/source.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (artist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow);

        dbContext.Artists.Add(artist);
        dbContext.Albums.Add(album);
        dbContext.Tracks.Add(track);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteStudioArtistTrackHandler(
            dbContext,
            _currentUserServiceMock.Object,
            TimeProvider.System);

        await handler.Handle(new DeleteStudioArtistTrackCommand(track.Id), CancellationToken.None);

        var deletedTrack = await dbContext.Tracks
            .IgnoreQueryFilters()
            .FirstAsync(t => t.Id == track.Id);

        deletedTrack.IsDeleted.Should().BeTrue();
        deletedTrack.DeletedAt.Should().NotBeNull();
        deletedTrack.UpdatedAt.Should().BeOnOrAfter(utcNow);

        dbContext.OutboxMessages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ThrowForbidden_When_TrackBelongsToAnotherArtist()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var user = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(user);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        var currentArtist = Artist.Create(user.Id, "Current Artist", null, null, null, VerificationStatus.Verified, utcNow);
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

        var handler = new DeleteStudioArtistTrackHandler(
            dbContext,
            _currentUserServiceMock.Object,
            TimeProvider.System);

        var action = async () => await handler.Handle(
            new DeleteStudioArtistTrackCommand(track.Id),
            CancellationToken.None);

        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*current artist profile*");
    }
}
