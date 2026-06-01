using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Features.Client.Home.Queries.GetSystemTopTracks;
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Home;

public class GetSystemTopTracksHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<TimeProvider> _timeProviderMock;

    public GetSystemTopTracksHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _cacheServiceMock = new Mock<ICacheService>();
        _timeProviderMock = new Mock<TimeProvider>();
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_Should_ReturnTracksFromCache_WhenCacheHit()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        
        var cachedTracks = new List<TopTrackDto>
        {
            new TopTrackDto(Guid.NewGuid(), "Cached Track", new List<TrackArtistDto>(), null)
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<TopTrackDto>>("system_top_tracks_limit_10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedTracks);

        var handler = new GetSystemTopTracksHandler(dbContext, _cacheServiceMock.Object, _timeProviderMock.Object);

        // Act
        var result = await handler.Handle(new GetSystemTopTracksQuery(Limit: 10), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Cached Track");
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_SetCache_WhenCacheMiss()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        
        _cacheServiceMock.Setup(x => x.GetAsync<List<TopTrackDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<TopTrackDto>?)null);

        var handler = new GetSystemTopTracksHandler(dbContext, _cacheServiceMock.Object, _timeProviderMock.Object);

        // Act
        await handler.Handle(new GetSystemTopTracksQuery(Limit: 5), CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(x => x.GetAsync<List<TopTrackDto>>("system_top_tracks_limit_5", It.IsAny<CancellationToken>()), Times.Once);
    }
}