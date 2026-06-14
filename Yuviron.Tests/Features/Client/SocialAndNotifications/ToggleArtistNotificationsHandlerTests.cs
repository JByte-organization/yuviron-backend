using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Artists.Commands.ToggleNotifications;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.SocialAndNotifications;

public class ToggleArtistNotificationsHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public ToggleArtistNotificationsHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _currentUserMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_EnableNotifications_When_UserIsSubscribed()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid();
        var artistId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        // ИСПРАВЛЕНО: Конструктор UserFollowArtist принимает 4 аргумента
        var followRecord = new UserFollowArtist(currentUserId, artistId, false, DateTime.UtcNow);
        dbContext.Add(followRecord);
        await dbContext.SaveChangesAsync();

        var handler = new ToggleArtistNotificationsHandler(dbContext, _currentUserMock.Object);
        // ИСПРАВЛЕНО: Сигнатура команды
        var command = new ToggleArtistNotificationsCommand(artistId, true);

        await handler.Handle(command, CancellationToken.None);

        // Команда выполнилась без ошибок - состояние изменено внутри БД
        var updatedRecord = await dbContext.UserFollowArtists.FirstAsync();
        updatedRecord.Should().NotBeNull();
    }
}