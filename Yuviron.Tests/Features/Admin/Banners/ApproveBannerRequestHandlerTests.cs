using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.Banners.Commands.ApproveBannerRequest;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Admin.Banners;

public class ApproveBannerRequestHandlerTests
{
    [Fact]
    public async Task Handle_Should_Approve_BannerRequest_Without_Album_And_Create_Artist_SmartLink()
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
            utcNow);

        dbContext.Users.Add(user);
        dbContext.Artists.Add(artist);
        dbContext.BannerRequests.Add(bannerRequest);
        await dbContext.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(adminId);

        var eventBusMock = new Mock<IEventBus>();
        var handler = new ApproveBannerRequestHandler(dbContext, TimeProvider.System, currentUserMock.Object, eventBusMock.Object);

        var bannerId = await handler.Handle(new ApproveBannerRequestCommand(bannerRequest.Id, SortOrder: 1, IsActive: true), CancellationToken.None);

        var approvedRequest = await dbContext.BannerRequests.FindAsync(bannerRequest.Id);
        approvedRequest.Should().NotBeNull();
        approvedRequest!.Status.Should().Be(BannerRequestStatus.Approved);

        var banner = await dbContext.Banners.FindAsync(bannerId);
        banner.Should().NotBeNull();
        banner!.ArtistId.Should().Be(artist.Id);
        banner.StartsAtUtc.Should().BeNull();

        var smartLink = await dbContext.SmartLinks.SingleAsync();
        smartLink.EntityType.Should().Be(SmartLinkType.Artist);
        smartLink.EntityId.Should().Be(artist.Id);
    }
}
