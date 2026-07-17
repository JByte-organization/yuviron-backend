using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Features.Client.Marketing.Queries.ResolvePublicSmartLink;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Marketing;

public class ResolvePublicSmartLinkHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Handle_Should_ReturnAlbumFrontendPath_And_RecordClick_When_CodeMatchesEntity()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;
        var album = Album.Create(
            "Test Album",
            null,
            null,
            utcNow.Date,
            ReleaseType.Album,
            VisibilityStatus.Published,
            null,
            Array.Empty<Guid>(),
            utcNow);
        var smartLink = SmartLink.Create("abc12345", SmartLinkType.Album, album.Id, null, null, utcNow);

        dbContext.AddRange(album, smartLink);
        await dbContext.SaveChangesAsync();

        var handler = new ResolvePublicSmartLinkHandler(dbContext, dbContext, dbContext, dbContext, TimeProvider.System);

        var result = await handler.Handle(
            new ResolvePublicSmartLinkQuery(
                SmartLinkType.Album,
                album.PublicId,
                "ABC12345",
                "ua",
                "https://example.com",
                "mobile"),
            CancellationToken.None);

        result.RelativePath.Should().Be($"/albums/{album.Id}");

        var click = await dbContext.Set<SmartLinkClick>().SingleAsync();
        click.SmartLinkId.Should().Be(smartLink.Id);
        click.CountryCode.Should().Be("UA");
        click.Referrer.Should().Be("https://example.com");
        click.DeviceType.Should().Be("mobile");
    }

    [Fact]
    public async Task Handle_Should_ReturnEntityPath_WithoutRecordingClick_When_CodeDoesNotMatch()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var artist = Artist.Create(null, "Test Artist", null, null, null, VerificationStatus.None, DateTime.UtcNow);

        dbContext.Add(artist);
        await dbContext.SaveChangesAsync();

        var handler = new ResolvePublicSmartLinkHandler(dbContext, dbContext, dbContext, dbContext, TimeProvider.System);

        var result = await handler.Handle(
            new ResolvePublicSmartLinkQuery(
                SmartLinkType.Artist,
                artist.PublicId,
                "missing",
                null,
                null,
                null),
            CancellationToken.None);

        result.RelativePath.Should().Be($"/artists/{artist.Id}");
        dbContext.Set<SmartLinkClick>().Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_Should_ReturnUserProfilePath_When_ResolvingProfileLink()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;

        var user = User.Create("profile@example.com", "hash", "Profile", false, true, utcNow);
        var profile = UserProfile.Create(user.Id, "Profile", null, null, null, null, null, utcNow, Gender.Male, utcNow);
        user.SetProfile(profile);
        var smartLink = SmartLink.Create("user123", SmartLinkType.UserProfile, user.Id, null, null, utcNow);

        dbContext.Add(user);
        dbContext.Add(smartLink);
        await dbContext.SaveChangesAsync();

        var handler = new ResolvePublicSmartLinkHandler(dbContext, dbContext, dbContext, dbContext, TimeProvider.System);

        var result = await handler.Handle(
            new ResolvePublicSmartLinkQuery(
                SmartLinkType.UserProfile,
                profile.PublicId,
                "user123",
                "us",
                null,
                "desktop"),
            CancellationToken.None);

        result.RelativePath.Should().Be($"/users/{user.Id}");

        var click = await dbContext.Set<SmartLinkClick>().SingleAsync();
        click.SmartLinkId.Should().Be(smartLink.Id);
        click.CountryCode.Should().Be("US");
        click.DeviceType.Should().Be("desktop");
    }
}
