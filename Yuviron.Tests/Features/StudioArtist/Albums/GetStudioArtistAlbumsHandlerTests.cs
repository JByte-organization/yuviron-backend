using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.GetAlbums;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Albums;

public class GetStudioArtistAlbumsHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetStudioArtistAlbumsHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_ReturnOnlyCurrentArtistAlbums_WithPagination()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var user = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(user);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        var currentArtist = Artist.Create(user.Id, "Current Artist", null, null, null, VerificationStatus.Verified, utcNow);
        var foreignArtist = Artist.Create(null, "Foreign Artist", null, null, null, VerificationStatus.None, utcNow);

        var currentAlbumOldest = Album.Create(
            "Oldest Album",
            null,
            null,
            utcNow.Date.AddDays(-30),
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            new[] { currentArtist.Id },
            utcNow.AddMinutes(-30));

        var currentAlbumMiddle = Album.Create(
            "Middle Album",
            null,
            null,
            utcNow.Date.AddDays(-20),
            ReleaseType.Album,
            VisibilityStatus.Draft,
            null,
            new[] { currentArtist.Id },
            utcNow.AddMinutes(-20));

        var currentAlbumNewest = Album.Create(
            "Newest Album",
            null,
            null,
            utcNow.Date.AddDays(-10),
            ReleaseType.Album,
            VisibilityStatus.Scheduled,
            utcNow.AddDays(10),
            new[] { currentArtist.Id },
            utcNow.AddMinutes(-10));

        var currentSingle = Album.Create(
            "Single Release",
            null,
            null,
            utcNow.Date.AddDays(-5),
            ReleaseType.Single,
            VisibilityStatus.Published,
            null,
            new[] { currentArtist.Id },
            utcNow.AddMinutes(-5));

        var foreignAlbum = Album.Create(
            "Foreign Album",
            null,
            null,
            utcNow.Date.AddDays(-1),
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            new[] { foreignArtist.Id },
            utcNow.AddMinutes(-1));

        var oldestTrack = Track.Create(
            Guid.NewGuid(),
            currentAlbumOldest.Id,
            1,
            "Track 1",
            180000,
            false,
            null,
            "tracks/oldest/source.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (currentArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow);
        oldestTrack.AddPlays(10);

        var middleTrack = Track.Create(
            Guid.NewGuid(),
            currentAlbumMiddle.Id,
            1,
            "Track 2",
            180000,
            false,
            null,
            "tracks/middle/source.mp3",
            VisibilityStatus.Draft,
            null,
            new[] { (currentArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow);
        middleTrack.AddPlays(25);

        var newestTrack = Track.Create(
            Guid.NewGuid(),
            currentAlbumNewest.Id,
            1,
            "Track 3",
            180000,
            false,
            null,
            "tracks/newest/source.mp3",
            VisibilityStatus.Scheduled,
            null,
            new[] { (currentArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow);
        newestTrack.AddPlays(50);

        var singleTrack = Track.Create(
            Guid.NewGuid(),
            currentSingle.Id,
            1,
            "Single Track",
            180000,
            false,
            null,
            "tracks/single/source.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (currentArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow);

        var foreignTrack = Track.Create(
            Guid.NewGuid(),
            foreignAlbum.Id,
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
        foreignTrack.AddPlays(500);

        dbContext.Artists.AddRange(currentArtist, foreignArtist);
        dbContext.Albums.AddRange(currentAlbumOldest, currentAlbumMiddle, currentAlbumNewest, currentSingle, foreignAlbum);
        dbContext.Tracks.AddRange(oldestTrack, middleTrack, newestTrack, singleTrack, foreignTrack);
        await dbContext.SaveChangesAsync();

        var handler = new GetStudioArtistAlbumsHandler(dbContext, _currentUserServiceMock.Object);

        var result = await handler.Handle(new GetStudioArtistAlbumsQuery(Page: 2, PageSize: 2), CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.Items.Should().HaveCount(1);

        var album = result.Items.Single();
        album.Title.Should().Be("Oldest Album");
        album.TracksCount.Should().Be(1);
        album.TotalPlays.Should().Be(10);
        album.Artists.Should().ContainSingle(x => x.Id == currentArtist.Id && x.Name == "Current Artist");
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_When_UserHasNoArtistMembership()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var user = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        var handler = new GetStudioArtistAlbumsHandler(dbContext, _currentUserServiceMock.Object);

        var action = async () => await handler.Handle(new GetStudioArtistAlbumsQuery(), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*for user*");
    }
}
