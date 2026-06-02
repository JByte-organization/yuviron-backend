using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.StudioArtist.Dashboard.Queries.GetDashboardStats;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Dashboard;

public class GetStudioArtistDashboardHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetStudioArtistDashboardHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_ReturnDashboard_ForCurrentOwnedArtist()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var currentUser = User.Create("owner@mail.com", "hash", "Owner", true, true, utcNow);
        var teamMember = User.Create("manager@mail.com", "hash", "Manager", true, true, utcNow);
        var followerOne = User.Create("follower1@mail.com", "hash", "Follower One", true, true, utcNow);
        var followerTwo = User.Create("follower2@mail.com", "hash", "Follower Two", true, true, utcNow);

        dbContext.Users.AddRange(currentUser, teamMember, followerOne, followerTwo);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        var ownedArtist = Artist.Create(
            currentUser.Id,
            "Owned Artist",
            "Bio",
            "avatars/owned",
            "banners/owned",
            VerificationStatus.Verified,
            utcNow);
        ownedArtist.AddTeamMember(teamMember.Id, ArtistTeamRole.Manager, utcNow);
        ownedArtist.AddPlays(1500);
        ownedArtist.SetMonthlyListenersCount(320);

        var managedArtist = Artist.Create(
            null,
            "Managed Artist",
            null,
            null,
            null,
            VerificationStatus.None,
            utcNow.AddMinutes(-10));
        managedArtist.AddTeamMember(currentUser.Id, ArtistTeamRole.Manager, utcNow.AddMinutes(-10));
        managedArtist.AddPlays(9999);
        managedArtist.SetMonthlyListenersCount(999);

        var ownedAlbum = Album.Create(
            "Owned Album",
            null,
            null,
            utcNow.Date,
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            new[] { ownedArtist.Id },
            utcNow);
        var managedAlbum = Album.Create(
            "Managed Album",
            null,
            null,
            utcNow.Date,
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            new[] { managedArtist.Id },
            utcNow);

        var ownedTrack = Track.Create(
            Guid.NewGuid(),
            ownedAlbum.Id,
            1,
            "Owned Track",
            180000,
            false,
            null,
            "tracks/owned/audio.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (ownedArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow);
        ownedTrack.MarkAsReady("tracks/owned/playlist.m3u8", "tracks/owned/audio.mp3", utcNow);

        var managedTrack = Track.Create(
            Guid.NewGuid(),
            managedAlbum.Id,
            1,
            "Managed Track",
            180000,
            false,
            null,
            "tracks/managed/audio.mp3",
            VisibilityStatus.Published,
            null,
            new[] { (managedArtist.Id, ArtistRole.Main) },
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            utcNow);
        managedTrack.MarkAsReady("tracks/managed/playlist.m3u8", "tracks/managed/audio.mp3", utcNow);

        var plan = Plan.Create("Artist Pro", 14.99m, "USD", PlanPeriod.Month, PlanType.Artist, utcNow);
        var subscription = ArtistSubscription.Create(
            ownedArtist.Id,
            currentUser.Id,
            plan.Id,
            utcNow.AddDays(-5),
            utcNow.AddDays(25),
            SubscriptionStatus.Active,
            utcNow,
            "sub_123");

        dbContext.Artists.AddRange(ownedArtist, managedArtist);
        dbContext.Albums.AddRange(ownedAlbum, managedAlbum);
        dbContext.Tracks.AddRange(ownedTrack, managedTrack);
        dbContext.Plans.Add(plan);
        dbContext.ArtistSubscriptions.Add(subscription);
        dbContext.UserFollowArtists.AddRange(
            new UserFollowArtist(followerOne.Id, ownedArtist.Id, true, utcNow),
            new UserFollowArtist(followerTwo.Id, ownedArtist.Id, false, utcNow),
            new UserFollowArtist(followerOne.Id, managedArtist.Id, true, utcNow));

        await dbContext.SaveChangesAsync();

        var handler = new GetStudioArtistDashboardHandler(
            dbContext,
            _currentUserServiceMock.Object,
            TimeProvider.System);

        var result = await handler.Handle(new GetStudioArtistDashboardQuery(), CancellationToken.None);

        result.Artist.Id.Should().Be(ownedArtist.Id);
        result.Artist.Name.Should().Be("Owned Artist");
        result.Artist.CurrentUserRole.Should().Be(ArtistTeamRole.Owner);
        result.Summary.TotalAlbums.Should().Be(1);
        result.Summary.TotalTracks.Should().Be(1);
        result.Summary.TotalFollowers.Should().Be(2);
        result.Summary.TeamMembersCount.Should().Be(2);
        result.Summary.TotalPlays.Should().Be(1500);
        result.Summary.MonthlyListenersCount.Should().Be(320);
        result.ActiveSubscription.Should().NotBeNull();
        result.ActiveSubscription!.PlanName.Should().Be("Artist Pro");
        result.ActiveSubscription.IsAutoRenewing.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_WhenUserHasNoArtistMembership()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var currentUser = User.Create("owner@mail.com", "hash", "Owner", true, true, utcNow);
        dbContext.Users.Add(currentUser);
        await dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        var handler = new GetStudioArtistDashboardHandler(
            dbContext,
            _currentUserServiceMock.Object,
            TimeProvider.System);

        var action = async () => await handler.Handle(new GetStudioArtistDashboardQuery(), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
