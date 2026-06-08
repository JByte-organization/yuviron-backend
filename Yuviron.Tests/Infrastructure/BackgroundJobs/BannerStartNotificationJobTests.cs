using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.BackgroundJobs;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Infrastructure.BackgroundJobs;

public class BannerStartNotificationJobTests
{
    [Fact]
    public async Task RunOnceAsync_Should_Publish_Event_And_Mark_Banner_As_Notified()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var utcNow = new DateTime(2026, 6, 8, 12, 0, 0, DateTimeKind.Utc);
        var banner = Banner.Create(
            "Promo Banner",
            "https://cdn/banner.jpg",
            "https://example.com",
            1,
            true,
            utcNow,
            Guid.NewGuid(),
            utcNow.AddMinutes(-5));

        dbContext.Banners.Add(banner);
        await dbContext.SaveChangesAsync();

        var eventBusMock = new Mock<IEventBus>();
        eventBusMock
            .Setup(x => x.PublishAsync(It.IsAny<BannerStartedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var providerMock = new Mock<IServiceProvider>();
        providerMock.Setup(x => x.GetService(typeof(AppDbContext))).Returns(dbContext);
        providerMock.Setup(x => x.GetService(typeof(IEventBus))).Returns(eventBusMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.SetupGet(x => x.ServiceProvider).Returns(providerMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(utcNow));

        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<BannerStartNotificationJob>>();
        var job = new BannerStartNotificationJob(scopeFactoryMock.Object, timeProviderMock.Object, loggerMock.Object);

        await job.RunOnceAsync(CancellationToken.None);

        eventBusMock.Verify(x => x.PublishAsync(
            It.Is<BannerStartedEvent>(e => e.BannerId == banner.Id && e.BannerTitle == "Promo Banner"),
            It.IsAny<CancellationToken>()), Times.Once);

        var reloadedBanner = await dbContext.Banners.FindAsync(banner.Id);
        reloadedBanner.Should().NotBeNull();
        reloadedBanner!.StartNotificationSentAtUtc.Should().Be(utcNow);
    }
}
