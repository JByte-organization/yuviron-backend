using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.BackgroundJobs;
using Yuviron.Infrastructure.Persistence;
using Yuviron.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Tests.Infrastructure.BackgroundJobs;

public class BannerStartNotificationJobTests
{
    [Fact]
    public async Task RunOnceAsync_Should_Publish_BannerStartedEvent_And_Mark_As_Sent()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var utcNow = new DateTime(2026, 6, 8, 12, 0, 0, DateTimeKind.Utc);
        
        var artistId = Guid.NewGuid();
        var banner = Banner.Create(
            "Campaign",
            "url",
            "target",
            true,
            utcNow.AddHours(-1),
            artistId,
            startsAtUtc: utcNow.AddMinutes(-5));

        dbContext.Banners.Add(banner);
        await dbContext.SaveChangesAsync();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(dbContext);
        
        var eventBusMock = new Mock<IEventBus>();
        serviceCollection.AddSingleton(eventBusMock.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider);
        scopeFactoryMock.Setup(s => s.CreateScope()).Returns(scopeMock.Object);

        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(utcNow));

        var job = new BannerStartNotificationJob(
            scopeFactoryMock.Object,
            timeProviderMock.Object,
            new Mock<ILogger<BannerStartNotificationJob>>().Object);

        await job.RunOnceAsync(CancellationToken.None);

        eventBusMock.Verify(x => x.PublishAsync(
            It.Is<BannerStartedEvent>(e => e.ArtistId == artistId && e.BannerId == banner.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        var updatedBanner = await dbContext.Banners.FindAsync(banner.Id);
        updatedBanner!.StartNotificationSentAtUtc.Should().NotBeNull();
    }
}
