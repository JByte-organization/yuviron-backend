using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateAudioQuality;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateCrossfade;
using Yuviron.Application.Features.Client.Settings.Commands.TogglePrivateSession;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateTheme;
using Yuviron.Application.Features.Client.Users.Commands.UpdateAccountDetails;
using Yuviron.Application.Features.Client.Users.Commands.UpdateMarketingPreferences;
using Yuviron.Application.Policies;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Settings;

public class AccountSettingsHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task UpdateTheme_Should_UpdateThemeMode()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        dbContext.UserSettings.Add(UserSettings.Create(userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = Mock.Of<ICurrentUserService>(s => s.UserId == userId);
        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(userId, AppPermission.CustomTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var policy = new UserSettingsPolicy(Options.Create(new AudioSettingsOptions()));

        var handler = new UpdateThemeCommandHandler(dbContext, currentUser, permissionService.Object, policy);

        await handler.Handle(new UpdateThemeCommand(ThemeMode.Dark), CancellationToken.None);

        var updated = await dbContext.UserSettings.FindAsync(userId);
        updated.Should().NotBeNull();
        updated!.ThemeMode.Should().Be(ThemeMode.Dark.ToString());
    }

    [Fact]
    public async Task UpdateAudioQuality_Should_SanitizeQuality_ForFreeUser()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        dbContext.UserSettings.Add(UserSettings.Create(userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(userId, AppPermission.PlayerHighQuality, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var policy = new UserSettingsPolicy(Options.Create(new AudioSettingsOptions { HlsQualities = new[] { 128, 320 } }));
        var handler = new UpdateAudioQualityCommandHandler(dbContext, currentUser.Object, permissionService.Object, policy);

        await handler.Handle(new UpdateAudioQualityCommand(320), CancellationToken.None);

        var updated = await dbContext.UserSettings.FindAsync(userId);
        updated.Should().NotBeNull();
        updated!.AudioQualityPreference.Should().Be(128);
        updated.CrossfadeMs.Should().Be(0);
    }

    [Fact]
    public async Task UpdateCrossfade_Should_UpdateOnlyCrossfade()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        dbContext.UserSettings.Add(UserSettings.Create(userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var handler = new UpdateCrossfadeCommandHandler(dbContext, currentUser.Object);

        await handler.Handle(new UpdateCrossfadeCommand(1500), CancellationToken.None);

        var updated = await dbContext.UserSettings.FindAsync(userId);
        updated.Should().NotBeNull();
        updated!.AudioQualityPreference.Should().Be(128);
        updated.CrossfadeMs.Should().Be(1500);
    }

    [Fact]
    public async Task TogglePrivateSession_Should_ThrowForbidden_When_UserLacksPermission()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        dbContext.UserSettings.Add(UserSettings.Create(userId, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(userId, AppPermission.PrivateSession, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var policy = new UserSettingsPolicy(Options.Create(new AudioSettingsOptions()));
        var handler = new TogglePrivateSessionCommandHandler(dbContext, currentUser.Object, permissionService.Object, policy);

        Func<Task> action = () => handler.Handle(new TogglePrivateSessionCommand(true), CancellationToken.None);

        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Private session is a premium feature.");
    }

    [Fact]
    public async Task UpdateMarketingPreferences_Should_UpdateUserFlag()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;
        var user = User.Create("user@example.com", "hash", "Alex", acceptMarketing: false, acceptTerms: true, utcNow: utcNow);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(user.Id);

        var handler = new UpdateMarketingPreferencesCommandHandler(dbContext, currentUser.Object);

        await handler.Handle(new UpdateMarketingPreferencesCommand(true), CancellationToken.None);

        var updated = await dbContext.Users.FindAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.AcceptMarketing.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAccountDetails_Should_UpdateEmailAndProfileFields()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;
        var user = User.Create("old@example.com", "hash", "Alex", acceptMarketing: false, acceptTerms: true, utcNow: utcNow);
        user.SetProfile(UserProfile.Create(user.Id, "Alex", null, null, "MD", null, null, utcNow.AddYears(-20), Gender.Male, utcNow));
        dbContext.Users.Add(user);

        var other = User.Create("taken@example.com", "hash2", "Bob", acceptMarketing: false, acceptTerms: true, utcNow: utcNow);
        other.SetProfile(UserProfile.Create(other.Id, "Bob", null, null, "MD", null, null, utcNow.AddYears(-22), Gender.Male, utcNow));
        dbContext.Users.Add(other);
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(user.Id);

        var handler = new UpdateAccountDetailsHandler(dbContext, currentUser.Object);

        await handler.Handle(new UpdateAccountDetailsCommand(
            "new@example.com",
            "RO",
            utcNow.AddYears(-19),
            Gender.Female,
            true), CancellationToken.None);

        var updated = await dbContext.Users.Include(x => x.Profile).FirstAsync(x => x.Id == user.Id);
        updated.Email.Should().Be("new@example.com");
        updated.AcceptMarketing.Should().BeTrue();
        updated.Profile.Country.Should().Be("RO");
        updated.Profile.DateOfBirth.Date.Should().Be(utcNow.AddYears(-19).Date);
        updated.Profile.Gender.Should().Be(Gender.Female);
    }

    [Fact]
    public async Task UpdateAccountDetails_Should_Throw_When_EmailAlreadyTaken()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;
        var user = User.Create("old@example.com", "hash", "Alex", acceptMarketing: false, acceptTerms: true, utcNow: utcNow);
        user.SetProfile(UserProfile.Create(user.Id, "Alex", null, null, "MD", null, null, utcNow.AddYears(-20), Gender.Male, utcNow));
        dbContext.Users.Add(user);

        var other = User.Create("taken@example.com", "hash2", "Bob", acceptMarketing: false, acceptTerms: true, utcNow: utcNow);
        other.SetProfile(UserProfile.Create(other.Id, "Bob", null, null, "MD", null, null, utcNow.AddYears(-22), Gender.Male, utcNow));
        dbContext.Users.Add(other);
        await dbContext.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(user.Id);

        var handler = new UpdateAccountDetailsHandler(dbContext, currentUser.Object);

        Func<Task> action = () => handler.Handle(new UpdateAccountDetailsCommand(
            "taken@example.com",
            "RO",
            utcNow.AddYears(-19),
            Gender.Female,
            true), CancellationToken.None);

        await action.Should().ThrowAsync<UserAlreadyExistsException>();
    }
}
