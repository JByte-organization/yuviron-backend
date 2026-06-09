using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Client.Appearance.Commands.CreateThemePreset;
using Yuviron.Application.Features.Client.Appearance.Queries.GetThemeModes;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateTheme;
using Yuviron.Application.Policies;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Appearance;

public class ThemePresetHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task GetThemeModes_Should_ReturnAllModes()
    {
        var handler = new GetThemeModesHandler();

        var result = await handler.Handle(new GetThemeModesQuery(), CancellationToken.None);

        result.Should().ContainInOrder(ThemeMode.System, ThemeMode.Dark, ThemeMode.White);
    }

    [Fact]
    public async Task GetThemeModes_Should_ReturnAllModes_ForPremiumUser()
    {
        var handler = new GetThemeModesHandler();

        var result = await handler.Handle(new GetThemeModesQuery(), CancellationToken.None);

        result.Should().ContainInOrder(ThemeMode.System, ThemeMode.Dark, ThemeMode.White);
    }

    [Fact]
    public async Task CreateThemePreset_Should_CreateAndActivatePreset()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        dbContext.UserSettings.Add(UserSettings.Create(userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(userId, AppPermission.CustomTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateThemePresetCommandHandler(dbContext, currentUser.Object, permissionService.Object);

        var presetId = await handler.Handle(
            new CreateThemePresetCommand("Velvet", "#111111", "#222222", "#333333"),
            CancellationToken.None);

        var theme = await dbContext.Themes.FindAsync(presetId);
        theme.Should().NotBeNull();
        theme!.UserId.Should().Be(userId);
        theme.Name.Should().Be("Velvet");

        var settings = await dbContext.UserSettings.FindAsync(userId);
        settings!.ThemeId.Should().Be(presetId);
        settings.CustomThemeId.Should().BeNull();
    }

    [Fact]
    public async Task UpdateThemeMode_Should_AllowSystemMode_ForFreeUser()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        dbContext.UserSettings.Add(UserSettings.Create(userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(userId, AppPermission.CustomTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var policy = new UserSettingsPolicy(Options.Create(new AudioSettingsOptions()));
        var handler = new UpdateThemeCommandHandler(dbContext, currentUser.Object, permissionService.Object, policy);

        await handler.Handle(new UpdateThemeCommand(ThemeMode.System), CancellationToken.None);

        var updated = await dbContext.UserSettings.FindAsync(userId);
        updated.Should().NotBeNull();
        updated!.ThemeMode.Should().Be(ThemeMode.System.ToString());
    }
}
