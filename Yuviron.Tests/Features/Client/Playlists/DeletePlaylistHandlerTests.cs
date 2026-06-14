using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Playlists.Commands.DeletePlaylist;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Playlists;

public class DeletePlaylistHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly TimeProvider _timeProvider;

    public DeletePlaylistHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _currentUserMock = new Mock<ICurrentUserService>();
        _timeProvider = TimeProvider.System;
    }

    [Fact]
    public async Task Handle_Should_MarkAsDeleted()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var coverUrl = "s3://bucket/covers/playlist-xyz.jpg";
        var playlist = Playlist.Create(currentUserId, null, "To Be Deleted", null, coverUrl, PlaylistVisibility.Public, false, DateTime.UtcNow);
        dbContext.Add(playlist);
        await dbContext.SaveChangesAsync();

        // ИСПРАВЛЕНО: Убран EventBus (3 аргумента)
        var handler = new DeletePlaylistHandler(dbContext, _currentUserMock.Object, _timeProvider);

        await handler.Handle(new DeletePlaylistCommand(playlist.Id), CancellationToken.None);

        var deletedPlaylist = await dbContext.Set<Playlist>().FindAsync(playlist.Id);
        deletedPlaylist!.IsDeleted.Should().BeTrue(); 
    }
}