using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylists;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Admin.Playlists;

public class GetPlaylistsHandlerTests
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
        var playlist = Playlist.Create(userId, null, "Admin Picks", null, null, PlaylistVisibility.Public, false, DateTime.UtcNow);

        dbContext.Add(User.Create("admin-playlists@test.com", "hash", "Admin", true, true, DateTime.UtcNow));
        dbContext.Add(playlist);
        dbContext.Add(Track.Create(trackId, Guid.NewGuid(), 1, "Jamando", 180000, false, null, "key", VisibilityStatus.Published, null, Array.Empty<(Guid, ArtistRole)>(), Array.Empty<Guid>(), Array.Empty<Guid>(), DateTime.UtcNow));
        dbContext.Add(new PlaylistTrack(playlist.Id, trackId, 1, userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var handler = new GetPlaylistsHandler(dbContext);

        var result = await handler.Handle(new GetPlaylistsQuery(TrackId: trackId), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].ContainsTrack.Should().BeTrue();
    }
}
