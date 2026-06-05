using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Playlists;

public class CreatePlaylistHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly TimeProvider _timeProvider;

    public CreatePlaylistHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _currentUserMock = new Mock<ICurrentUserService>();
        _timeProvider = TimeProvider.System;
    }

    [Fact]
    public async Task Handle_Should_CreatePlaylist_ForCurrentUser()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var handler = new CreatePlaylistHandler(dbContext, _timeProvider, _currentUserMock.Object);
        
        // ИСПРАВЛЕНО: Убрали Description. Используем только те параметры, которые есть в команде.
        var command = new CreatePlaylistCommand(
            Title: "My Summer Hits", 
            Visibility: PlaylistVisibility.Public, 
            CoverFileId: null);

        var newPlaylistId = await handler.Handle(command, CancellationToken.None);

        newPlaylistId.Should().NotBeEmpty();
        var playlistInDb = await dbContext.Playlists.FindAsync(newPlaylistId);
        playlistInDb.Should().NotBeNull();
        playlistInDb!.Title.Should().Be("My Summer Hits");
        playlistInDb.UserId.Should().Be(currentUserId);
    }
}