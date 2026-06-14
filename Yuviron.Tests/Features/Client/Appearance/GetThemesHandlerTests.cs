using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Appearance.Queries.GetCustomTheme;
using Yuviron.Application.Features.Client.Appearance.Queries.GetThemes;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Appearance;

public class GetThemesHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Handle_Should_ReturnThemes_With_Selected_Flag()
    {
        await using var dbContext = new AppDbContext(CreateOptions());

        var userId = Guid.NewGuid();
        var theme = Theme.Create("Ocean", "#2DD4BF", "#0F766E", "#042F2E", true, false);
        dbContext.Add(theme);

        var settings = UserSettings.Create(userId, DateTime.UtcNow);
        settings.ApplyDesign(theme.Id, null, DateTime.UtcNow);
        dbContext.Add(settings);
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(userId, AppPermission.CustomTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new GetThemesHandler(dbContext, currentUser.Object, permissionService.Object);

        var result = await handler.Handle(new GetThemesQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result.Single().IsSelected.Should().BeTrue();
        result.Single().Name.Should().Be("Ocean");
        result.Single().PrimaryColor.Should().Be("#2DD4BF");
    }

    [Fact]
    public async Task Handle_Should_NotReturnPremiumThemes_For_FreeUser()
    {
        await using var dbContext = new AppDbContext(CreateOptions());

        var userId = Guid.NewGuid();
        dbContext.Add(UserSettings.Create(userId, DateTime.UtcNow));
        
        dbContext.Add(Theme.Create("Free Theme", "#111111", "#222222", "#333333", false, false));
        dbContext.Add(Theme.Create("Premium Theme", "#444444", "#555555", "#666666", false, true));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(userId, AppPermission.CustomTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new GetThemesHandler(dbContext, currentUser.Object, permissionService.Object);

        var result = await handler.Handle(new GetThemesQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result.Single().Name.Should().Be("Free Theme");
    }

    [Fact]
    public async Task Handle_Should_ReturnCurrentCustomTheme()
    {
        await using var dbContext = new AppDbContext(CreateOptions());

        var userId = Guid.NewGuid();
        var customTheme = CustomTheme.Create(userId, "#111111", "#222222", "#000000", DateTime.UtcNow);
        dbContext.Add(customTheme);

        var settings = UserSettings.Create(userId, DateTime.UtcNow);
        settings.ApplyDesign(null, customTheme.Id, DateTime.UtcNow);
        dbContext.Add(settings);
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var handler = new GetCustomThemeHandler(dbContext, currentUser.Object);

        var result = await handler.Handle(new GetCustomThemeQuery(), CancellationToken.None);

        result.PrimaryColor.Should().Be("#111111");
        result.BackgroundColor.Should().Be("#000000");
    }

    [Fact]
    public async Task Handle_Should_Not_ReturnOtherUsersPresets()
    {
        await using var dbContext = new AppDbContext(CreateOptions());

        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        dbContext.Add(UserSettings.Create(currentUserId, DateTime.UtcNow));
        dbContext.Add(UserSettings.Create(otherUserId, DateTime.UtcNow));

        dbContext.Add(Theme.Create("Mine", "#111111", "#222222", "#333333", false, true, currentUserId));
        dbContext.Add(Theme.Create("Theirs", "#444444", "#555555", "#666666", false, true, otherUserId));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(currentUserId);

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(currentUserId, AppPermission.CustomTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new GetThemesHandler(dbContext, currentUser.Object, permissionService.Object);

        var result = await handler.Handle(new GetThemesQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result.Single().Name.Should().Be("Mine");
    }
}
