using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateCustomTheme;
using Yuviron.Application.Policies;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Settings;

public class UpdateCustomThemeHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Handle_Should_ThrowForbidden_When_UserLacksPermission()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        dbContext.Add(UserSettings.Create(userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(userId, AppPermission.CustomTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var policy = new UserSettingsPolicy(Options.Create(new AudioSettingsOptions()));
        var handler = new UpdateCustomThemeCommandHandler(dbContext, currentUser.Object, permissionService.Object, policy);

        Func<Task> action = () => handler.Handle(
            new UpdateCustomThemeCommand("#111111", "#222222", "#000000"),
            CancellationToken.None);

        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Custom themes are a premium feature.");
    }

    [Fact]
    public async Task Handle_Should_CreateOrUpdateCustomTheme_AndAttachItToSettings()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        dbContext.Add(UserSettings.Create(userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(userId, AppPermission.CustomTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var policy = new UserSettingsPolicy(Options.Create(new AudioSettingsOptions()));
        var handler = new UpdateCustomThemeCommandHandler(dbContext, currentUser.Object, permissionService.Object, policy);

        await handler.Handle(
            new UpdateCustomThemeCommand("#111111", "#222222", "#000000"),
            CancellationToken.None);

        var settings = await dbContext.Set<UserSettings>().FindAsync(userId);
        settings.Should().NotBeNull();
        settings!.CustomThemeId.Should().NotBeNull();

        var theme = await dbContext.Set<CustomTheme>().FindAsync(settings.CustomThemeId!.Value);
        theme.Should().NotBeNull();
        theme!.UserId.Should().Be(userId);
        theme.PrimaryColor.Should().Be("#111111");
        theme.BackgroundColor.Should().Be("#000000");
    }
}
