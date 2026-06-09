using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Notifications.Commands.UpdatePreferences;
using Yuviron.Application.Features.Client.Notifications.Preferences;
using Yuviron.Application.Features.Client.Notifications.Queries.GetPreferences;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Notifications;

public class NotificationPreferencesHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task GetPreferences_Should_ReturnCatalog_WithStoredOverrides()
    {
        await using var dbContext = new AppDbContext(CreateOptions());

        var userId = Guid.NewGuid();
        dbContext.UserNotificationPreferences.Add(UserNotificationPreference.Create(
            userId,
            NotificationCategory.Music,
            "new_release",
            false,
            DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var handler = new GetNotificationPreferencesHandler(dbContext, currentUser.Object);

        var result = await handler.Handle(new GetNotificationPreferencesQuery(), CancellationToken.None);

        var music = result.Groups.Single(x => x.Category == NotificationCategory.Music);
        music.Items.Should().ContainSingle(x => x.Code == "new_release" && x.Enabled == false);
        music.Items.Should().Contain(x => x.Code == NotificationPreferenceCatalog.CategoryAllCode && x.Enabled == true);
    }

    [Fact]
    public async Task UpdatePreference_Should_CreateOverride_And_RemoveDefaultOverride()
    {
        await using var dbContext = new AppDbContext(CreateOptions());

        var userId = Guid.NewGuid();
        dbContext.UserSettings.Add(UserSettings.Create(userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var handler = new UpdateNotificationPreferenceHandler(dbContext, currentUser.Object);

        await handler.Handle(new UpdateNotificationPreferenceCommand(
            NotificationCategory.System,
            "new_device_login",
            false), CancellationToken.None);

        (await dbContext.UserNotificationPreferences.CountAsync()).Should().Be(1);

        await handler.Handle(new UpdateNotificationPreferenceCommand(
            NotificationCategory.System,
            "new_device_login",
            true), CancellationToken.None);

        (await dbContext.UserNotificationPreferences.CountAsync()).Should().Be(0);
    }
}
