using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Admin.Banners.Commands.ApproveBannerRequest;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Admin.Banners;

public class ApproveBannerRequestHandlerTests
{
    private readonly MarketingOptions _marketingOptions = new() { BannerDurationDays = 7 };

    [Fact]
    public async Task Handle_Should_Approve_BannerRequest_And_Set_Default_Expiration()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var utcNow = new DateTime(2026, 6, 8, 12, 0, 0, DateTimeKind.Utc);
        var adminId = Guid.NewGuid();
        var artist = Artist.Create(null, "Artist", null, null, null, VerificationStatus.None, utcNow);
        var user = User.Create("artist@mail.com", "hash", "Artist Owner", true, true, utcNow);
        var bannerRequest = BannerRequest.Create(
            artist.Id,
            user.Id,
            albumId: null,
            "Promo Campaign",
            "https://cdn/banner.jpg",
            durationDays: 10,
            targetCountries: "US",
            targetGenres: null,
            utcNow);
        bannerRequest.MarkAsPaid("intent", utcNow);

        dbContext.Add(user);
        dbContext.Add(artist);
        dbContext.Add(bannerRequest);
        await dbContext.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(adminId);

        var eventBusMock = new Mock<IEventBus>();
        var marketingOptionsMock = new Mock<IOptions<MarketingOptions>>();
        marketingOptionsMock.Setup(x => x.Value).Returns(_marketingOptions);

        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(utcNow));

        var handler = new ApproveBannerRequestHandler(
            dbContext, 
            timeProviderMock.Object, 
            currentUserMock.Object, 
            eventBusMock.Object,
            marketingOptionsMock.Object);

        var bannerId = await handler.Handle(new ApproveBannerRequestCommand(bannerRequest.Id, IsActive: true), CancellationToken.None);

        var approvedRequest = await dbContext.Set<BannerRequest>().FindAsync(bannerRequest.Id);
        approvedRequest!.Status.Should().Be(BannerRequestStatus.Approved);
        approvedRequest.EndsAtUtc.Should().Be(utcNow.AddDays(10));

        var banner = await dbContext.Set<Banner>().FindAsync(bannerId);
        banner.Should().NotBeNull();
        banner!.EndsAtUtc.Should().Be(utcNow.AddDays(10));
        banner.TargetCountries.Should().Be("US");
    }

    [Fact]
    public async Task Handle_Should_Use_Provided_EndsAtUtc_When_Available()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var utcNow = new DateTime(2026, 6, 8, 12, 0, 0, DateTimeKind.Utc);
        var customEndsAt = utcNow.AddDays(14);
        var adminId = Guid.NewGuid();
        var artist = Artist.Create(null, "Artist", null, null, null, VerificationStatus.None, utcNow);
        var user = User.Create("artist@mail.com", "hash", "Artist Owner", true, true, utcNow);
        var bannerRequest = BannerRequest.Create(artist.Id, user.Id, null, "Title", "url", 7, null, null, utcNow);
        bannerRequest.MarkAsPaid("intent", utcNow);

        dbContext.Add(artist);
        dbContext.Add(bannerRequest);
        await dbContext.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(adminId);

        var marketingOptionsMock = new Mock<IOptions<MarketingOptions>>();
        marketingOptionsMock.Setup(x => x.Value).Returns(_marketingOptions);

        var handler = new ApproveBannerRequestHandler(
            dbContext, TimeProvider.System, currentUserMock.Object, new Mock<IEventBus>().Object, marketingOptionsMock.Object);

        var bannerId = await handler.Handle(
            new ApproveBannerRequestCommand(bannerRequest.Id, IsActive: true, EndsAtUtc: customEndsAt), 
            CancellationToken.None);

        var banner = await dbContext.Set<Banner>().FindAsync(bannerId);
        banner!.EndsAtUtc.Should().Be(customEndsAt);
    }
}
