using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.ClaimProfile;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Profiles;

public class ClaimArtistProfileHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public ClaimArtistProfileHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_CreateVerificationRequest()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        dbContext.Add(user);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        // Ничейный артист
        var artist = Artist.Create(null, "The Beatles", null, null, null, VerificationStatus.None, utcNow);
        dbContext.Add(artist);
        await dbContext.SaveChangesAsync();

        var options = Options.Create(new ArtistLimitsOptions { FreeUserMaxProfiles = 1, PremiumUserMaxProfiles = 5 });
        var handler = new ClaimArtistProfileHandler(dbContext, dbContext, dbContext, dbContext, TimeProvider.System, options, _currentUserServiceMock.Object);

        var command = new ClaimArtistProfileCommand(
            artist.Id, ClaimRole.Manager, "manager@beatles.com", "inst.com/beatles", null, "Please verify");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var request = await dbContext.VerificationRequests.FirstOrDefaultAsync(vr => vr.ArtistId == artist.Id);
        request.Should().NotBeNull();
        request!.Status.Should().Be(VerificationRequestStatus.Pending);
        request.OfficialEmail.Should().Be("manager@beatles.com");
    }

    [Fact]
    public async Task Handle_Should_ThrowException_When_ArtistAlreadyHasOwner()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        dbContext.Add(user);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        // Создаем артиста и сразу даем ему владельца (другого юзера)
        var artist = Artist.Create(Guid.NewGuid(), "Taken Band", null, null, null, VerificationStatus.Verified, utcNow);
        dbContext.Add(artist);
        await dbContext.SaveChangesAsync();

        var options = Options.Create(new ArtistLimitsOptions { FreeUserMaxProfiles = 1, PremiumUserMaxProfiles = 5 });
        var handler = new ClaimArtistProfileHandler(dbContext, dbContext, dbContext, dbContext, TimeProvider.System, options, _currentUserServiceMock.Object);

        var command = new ClaimArtistProfileCommand(
            artist.Id, ClaimRole.Artist, "email@mail.com", "link", null, null);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been verified and claimed*");
    }
    
    [Fact]
    public async Task Handle_Should_ThrowException_When_UserAlreadyHasPendingRequest()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        dbContext.Add(user);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        var artist = Artist.Create(null, "The Beatles", null, null, null, VerificationStatus.None, utcNow);
        dbContext.Add(artist);

        // ИМИТИРУЕМ, что юзер УЖЕ подал заявку вчера и она висит в статусе Pending
        var existingRequest = VerificationRequest.Create(
            artist.Id, user.Id, ClaimRole.Manager, "m@m.com", "link", null, null, utcNow);
        dbContext.Add(existingRequest);
        
        await dbContext.SaveChangesAsync();

        var options = Options.Create(new ArtistLimitsOptions { FreeUserMaxProfiles = 1, PremiumUserMaxProfiles = 5 });
        var handler = new ClaimArtistProfileHandler(dbContext, dbContext, dbContext, dbContext, TimeProvider.System, options, _currentUserServiceMock.Object);

        // Пытаемся подать ЕЩЕ ОДНУ заявку на того же артиста
        var command = new ClaimArtistProfileCommand(
            artist.Id, ClaimRole.Manager, "m2@m.com", "link2", null, null);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You already have a pending verification request for this artist.");
    }
    
    [Fact]
    public async Task Handle_Should_ThrowForbidden_When_ClaimLimitExceeded()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        dbContext.Add(user);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        // Юзер УЖЕ владеет одной группой (исчерпал бесплатный лимит)
        var ownedArtist = Artist.Create(user.Id, "My First Band", null, null, null, VerificationStatus.None, utcNow);
        dbContext.Add(ownedArtist);

        // Артист, которого он хочет забрать
        var targetArtist = Artist.Create(null, "Target Band", null, null, null, VerificationStatus.None, utcNow);
        dbContext.Add(targetArtist);
        
        await dbContext.SaveChangesAsync();

        // Лимит 1 профиль
        var options = Options.Create(new ArtistLimitsOptions { FreeUserMaxProfiles = 1, PremiumUserMaxProfiles = 5 });
        var handler = new ClaimArtistProfileHandler(dbContext, dbContext, dbContext, dbContext, TimeProvider.System, options, _currentUserServiceMock.Object);

        var command = new ClaimArtistProfileCommand(
            targetArtist.Id, ClaimRole.Manager, "m@m.com", "link", null, null);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*Limit exceeded*");
    }
}