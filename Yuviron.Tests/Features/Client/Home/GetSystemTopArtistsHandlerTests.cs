using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Features.Client.Home.Queries.GetSystemTopArtists;
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopArtists;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Home;

public class GetSystemTopArtistsHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICacheService> _cacheServiceMock;

    public GetSystemTopArtistsHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _cacheServiceMock = new Mock<ICacheService>();
    }

    [Fact]
    public async Task Handle_Should_ReturnTopArtists_OrderedByMonthlyListeners_WhenCacheIsMiss()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var artist1 = Artist.Create(null, "Rookie Artist", null, null, null, VerificationStatus.None, utcNow);
        artist1.SetMonthlyListenersCount(100);

        var artist2 = Artist.Create(null, "Global Superstar", null, null, null, VerificationStatus.Verified, utcNow);
        artist2.SetMonthlyListenersCount(5000000);

        var artist3 = Artist.Create(null, "Mid-tier Artist", null, null, null, VerificationStatus.None, utcNow);
        artist3.SetMonthlyListenersCount(15000);

        dbContext.Artists.AddRange(artist1, artist2, artist3);
        await dbContext.SaveChangesAsync();

        _cacheServiceMock.Setup(x => x.GetAsync<List<TopArtistDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<TopArtistDto>?)null);

        var handler = new GetSystemTopArtistsHandler(dbContext, _cacheServiceMock.Object);
        var query = new GetSystemTopArtistsQuery(Limit: 2); 

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2); 
        result[0].Name.Should().Be("Global Superstar"); 
        result[1].Name.Should().Be("Mid-tier Artist");  

        _cacheServiceMock.Verify(x => x.SetAsync(
            "system_top_artists_limit_2", 
            It.IsAny<List<TopArtistDto>>(), 
            TimeSpan.FromDays(1), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnArtistsFromCache_WhenCacheHit()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions); 
        
        var cachedArtists = new List<TopArtistDto>
        {
            new TopArtistDto(Guid.NewGuid(), "Cached Artist", null, 999)
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<TopArtistDto>>("system_top_artists_limit_5", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedArtists);

        var handler = new GetSystemTopArtistsHandler(dbContext, _cacheServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetSystemTopArtistsQuery(Limit: 5), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Cached Artist");

        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}