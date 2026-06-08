using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Features.Client.Home.Queries.GetHomeBanners;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Home;

public class GetHomeBannersHandlerTests
{
    [Fact]
    public async Task Handle_Should_Exclude_Banners_With_Future_Start_Date()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var timeProvider = new Mock<TimeProvider>();
        var utcNow = new DateTime(2026, 6, 8, 12, 0, 0, DateTimeKind.Utc);
        timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(utcNow));

        var activeNow = Banner.Create("Live Banner", "https://cdn/live.jpg", "https://example.com/live", 1, true, utcNow, null, utcNow.AddHours(-1));
        var activeFuture = Banner.Create("Future Banner", "https://cdn/future.jpg", "https://example.com/future", 2, true, utcNow, null, utcNow.AddHours(1));
        var inactive = Banner.Create("Inactive Banner", "https://cdn/inactive.jpg", "https://example.com/inactive", 3, false, utcNow, null, utcNow.AddHours(-1));

        dbContext.Banners.AddRange(activeNow, activeFuture, inactive);
        await dbContext.SaveChangesAsync();

        var handler = new GetHomeBannersHandler(dbContext, timeProvider.Object);

        var result = await handler.Handle(new GetHomeBannersQuery(Limit: 10), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Live Banner");
    }
}
