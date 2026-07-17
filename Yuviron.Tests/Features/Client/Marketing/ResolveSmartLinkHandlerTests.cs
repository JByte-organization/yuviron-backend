using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Features.Client.Marketing.Queries.ResolveSmartLink;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Marketing;

public class ResolveSmartLinkHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Handle_Should_ReturnAlbumPath_And_RecordClick()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var albumId = Guid.NewGuid();
        var smartLink = SmartLink.Create(
            "abc12345",
            SmartLinkType.Album,
            albumId,
            createdByUserId: null,
            expiresAt: null,
            utcNow: DateTime.UtcNow);

        dbContext.Add(smartLink);
        await dbContext.SaveChangesAsync();

        var handler = new ResolveSmartLinkHandler(dbContext, TimeProvider.System);

        var result = await handler.Handle(
            new ResolveSmartLinkQuery("ABC12345", "ua", "https://example.com", "mobile"),
            CancellationToken.None);

        result.RelativePath.Should().Be($"/albums/{albumId}");

        var click = await dbContext.Set<SmartLinkClick>().SingleAsync();
        click.SmartLinkId.Should().Be(smartLink.Id);
        click.CountryCode.Should().Be("UA");
        click.Referrer.Should().Be("https://example.com");
        click.DeviceType.Should().Be("mobile");
    }

    [Fact]
    public async Task Handle_Should_ReturnRoot_WithoutRecordingClick_When_LinkExpired()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var smartLink = SmartLink.Create(
            "expired1",
            SmartLinkType.Artist,
            Guid.NewGuid(),
            createdByUserId: null,
            expiresAt: DateTime.UtcNow.AddDays(-1),
            utcNow: DateTime.UtcNow.AddDays(-2));

        dbContext.Add(smartLink);
        await dbContext.SaveChangesAsync();

        var handler = new ResolveSmartLinkHandler(dbContext, TimeProvider.System);

        var result = await handler.Handle(
            new ResolveSmartLinkQuery("expired1", null, null, null),
            CancellationToken.None);

        result.RelativePath.Should().Be("/");
        dbContext.Set<SmartLinkClick>().Should().BeEmpty();
    }
}
