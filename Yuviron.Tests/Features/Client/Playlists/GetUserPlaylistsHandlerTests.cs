using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;
using Yuviron.Application.Features.Client.Playlists.Queries.GetUserPlaylists;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Playlists;

public class GetUserPlaylistsHandlerTests
{
    [Fact]
    public async Task Handle_Should_MarkPlaylist_When_TrackAlreadyExists()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var playlist = Playlist.Create(userId, null, "Music", null, null, PlaylistVisibility.Public, false, DateTime.UtcNow);
        dbContext.Playlists.Add(playlist);
        dbContext.Tracks.Add(Track.Create(trackId, Guid.NewGuid(), 1, "Jamando", 180000, false, null, "key", VisibilityStatus.Published, null, Array.Empty<(Guid, ArtistRole)>(), Array.Empty<Guid>(), Array.Empty<Guid>(), DateTime.UtcNow));
        dbContext.PlaylistTracks.Add(new PlaylistTrack(playlist.Id, trackId, 1, userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var handler = new GetUserPlaylistsHandler(dbContext, currentUser.Object);

        var result = await handler.Handle(new GetUserPlaylistsQuery(trackId), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].ContainsTrack.Should().BeTrue();
    }
}
