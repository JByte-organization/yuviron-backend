using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Playlists.Commands.RecoverPlaylist;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Tests.Features.Client.Playlists;

public class RecoverPlaylistHandlerTests
{
    [Fact]
    public async Task Handle_Should_Recover_Deleted_Playlist()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var utcNow = DateTime.UtcNow;
        var userId = Guid.NewGuid();

        var playlist = Playlist.Create(userId, null, "Deleted Playlist", null, null, PlaylistVisibility.Public, false, utcNow);
        playlist.Delete(utcNow);

        dbContext.Add(playlist);
        await dbContext.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(userId);

        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(utcNow.AddMinutes(5)));

        var handler = new RecoverPlaylistCommandHandler(dbContext, currentUserMock.Object, timeProviderMock.Object);

        await handler.Handle(new RecoverPlaylistCommand(playlist.Id), CancellationToken.None);

        var recoveredPlaylist = await dbContext.Set<Playlist>().FindAsync(playlist.Id);
        recoveredPlaylist.Should().NotBeNull();
        recoveredPlaylist!.IsDeleted.Should().BeFalse();
        recoveredPlaylist.UpdatedAt.Should().Be(utcNow.AddMinutes(5));
    }
}
