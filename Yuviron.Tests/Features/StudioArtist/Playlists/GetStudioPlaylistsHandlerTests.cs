using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylists;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Playlists;

public class GetStudioPlaylistsHandlerTests
{
    [Fact]
    public async Task Handle_Should_MarkPlaylist_When_TrackAlreadyExists()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);

        var utcNow = DateTime.UtcNow;
        var user = User.Create("studio@test.com", "hash", "Studio", true, true, utcNow);
        var userId = user.Id;
        var trackId = Guid.NewGuid();
        var artist = Artist.Create(null, "Studio Artist", null, null, null, VerificationStatus.None, utcNow);
        var playlist = Playlist.Create(null, artist.Id, "Studio Picks", null, null, PlaylistVisibility.Public, false, utcNow);

        dbContext.Add(user);
        dbContext.Add(artist);
        dbContext.Add(ArtistTeamMember.Create(artist.Id, userId, ArtistTeamRole.Owner, utcNow));
        dbContext.Add(playlist);
        dbContext.Add(Track.Create(trackId, Guid.NewGuid(), 1, "Jamando", 180000, false, null, "key", VisibilityStatus.Published, null, Array.Empty<(Guid, ArtistRole)>(), Array.Empty<Guid>(), Array.Empty<Guid>(), utcNow));
        dbContext.Add(new PlaylistTrack(playlist.Id, trackId, 1, userId, utcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        (await dbContext.ArtistTeamMembers.HasManagementAccess(artist.Id, userId).AnyAsync()).Should().BeTrue();

        var handler = new GetStudioPlaylistsHandler(dbContext, dbContext, currentUser.Object);

        var result = await handler.Handle(new GetStudioPlaylistsQuery(artist.Id, TrackId: trackId), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].ContainsTrack.Should().BeTrue();
    }
}
