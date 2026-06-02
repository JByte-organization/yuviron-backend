using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetTracks;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Tracks;

public class GetStudioArtistTracksHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetStudioArtistTracksHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_ReturnOnlyCurrentArtistTracks_WithSorting()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var currentUser = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(currentUser);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        var managedArtist = Artist.Create(null, "Managed Artist", null, null, null, VerificationStatus.None, utcNow.AddDays(-2));
        managedArtist.AddTeamMember(currentUser.Id, ArtistTeamRole.Manager, utcNow.AddDays(-2));

        var ownedArtist = Artist.Create(currentUser.Id, "Owned Artist", null, null, null, VerificationStatus.Verified, utcNow.AddDays(-1));

        var ownedAlbum = Album.Create(
            "Owned Album",
            null,
            "covers/owned.jpg",
            utcNow.Date,
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            new[] { ownedArtist.Id },
            utcNow);

        var managedAlbum = Album.Create(
            "Managed Album",
            null,
            "covers/managed.jpg",
            utcNow.Date,
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            new[] { managedArtist.Id },
            utcNow);

        var draftTrack = Track.Create(
            Guid.NewGuid(),
            ownedAlbum.Id,
            1,
            "Alpha Draft",
            200000,
            false,
            null,
            "tracks/owned-alpha/source.mp3",
            VisibilityStatus.Draft,
            null,
            new[] { (ownedArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow.AddMinutes(-10));

        var readyTrack = Track.Create(
            Guid.NewGuid(),
            ownedAlbum.Id,
            2,
            "Bravo Ready",
            180000,
            true,
            "tracks/owned-bravo/cover.jpg",
            "tracks/owned-bravo/source.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (ownedArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow.AddMinutes(-9));
        readyTrack.MarkAsReady("tracks/owned-bravo/playlist.m3u8", "tracks/owned-bravo/audio.mp3", utcNow.AddMinutes(-8));
        readyTrack.AddPlays(42);

        var foreignTrack = Track.Create(
            Guid.NewGuid(),
            managedAlbum.Id,
            1,
            "Aardvark Managed",
            210000,
            false,
            null,
            "tracks/managed/source.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (managedArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow.AddMinutes(-7));
        foreignTrack.MarkAsReady("tracks/managed/playlist.m3u8", "tracks/managed/audio.mp3", utcNow.AddMinutes(-6));

        dbContext.Artists.AddRange(managedArtist, ownedArtist);
        dbContext.Albums.AddRange(ownedAlbum, managedAlbum);
        dbContext.Tracks.AddRange(draftTrack, readyTrack, foreignTrack);
        await dbContext.SaveChangesAsync();

        var handler = new GetStudioArtistTracksHandler(dbContext, _currentUserServiceMock.Object);

        var result = await handler.Handle(
            new GetStudioArtistTracksQuery(SortBy: "Title", SortOrder: "asc", Page: 1, PageSize: 10),
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Title).Should().Equal("Alpha Draft", "Bravo Ready");
        result.Items.Should().OnlyContain(x => x.Artists.Any(a => a.Id == ownedArtist.Id));
        result.Items.Should().NotContain(x => x.Title == "Aardvark Managed");
        result.Items[0].VisibilityStatus.Should().Be(VisibilityStatus.Draft);
        result.Items[0].ProcessingStatus.Should().Be(TrackProcessingStatus.Processing);
        result.Items[1].ProcessingStatus.Should().Be(TrackProcessingStatus.Ready);
        result.Items[1].PlayCount.Should().Be(42);
    }

    [Fact]
    public async Task Handle_Should_ApplyPagination_ForCurrentArtistTracks()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var currentUser = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(currentUser);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        var artist = Artist.Create(currentUser.Id, "Owned Artist", null, null, null, VerificationStatus.None, utcNow);
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

        var firstTrack = Track.Create(
            Guid.NewGuid(),
            album.Id,
            1,
            "First Track",
            180000,
            false,
            null,
            "tracks/first/source.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (artist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow.AddMinutes(-5));

        var secondTrack = Track.Create(
            Guid.NewGuid(),
            album.Id,
            2,
            "Second Track",
            190000,
            false,
            null,
            "tracks/second/source.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (artist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow.AddMinutes(-4));

        dbContext.Artists.Add(artist);
        dbContext.Albums.Add(album);
        dbContext.Tracks.AddRange(firstTrack, secondTrack);
        await dbContext.SaveChangesAsync();

        var handler = new GetStudioArtistTracksHandler(dbContext, _currentUserServiceMock.Object);

        var result = await handler.Handle(
            new GetStudioArtistTracksQuery(SortBy: "Title", SortOrder: "asc", Page: 2, PageSize: 1),
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Second Track");
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_WhenUserHasNoArtistMembership()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var currentUser = User.Create("artist@mail.com", "hash", "Artist", true, true, utcNow);
        dbContext.Users.Add(currentUser);
        await dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        var handler = new GetStudioArtistTracksHandler(dbContext, _currentUserServiceMock.Object);

        var action = async () => await handler.Handle(new GetStudioArtistTracksQuery(), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
