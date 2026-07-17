using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Client.Marketing.Commands.CreateSmartLink;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Marketing;

public class CreateSmartLinkHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private readonly IOptions<FrontendOptions> _frontendOptions = Options.Create(new FrontendOptions { BaseUrl = "https://yuviron.com" });

    [Fact]
    public async Task Handle_Should_CreateSmartLink_When_ArtistExists()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;
        
        var user = User.Create("test@example.com", "hash", "Test", false, true, utcNow);
        var artist = Artist.Create(null, "Test Artist", null, null, null, VerificationStatus.None, utcNow);
        dbContext.AddRange(user, artist);
        await dbContext.SaveChangesAsync();

        var currentUser = Mock.Of<ICurrentUserService>(s => s.UserId == user.Id);
        
        var handler = new CreateSmartLinkHandler(
            dbContext, dbContext, dbContext, 
            dbContext,
            currentUser, TimeProvider.System, _frontendOptions);

        var result = await handler.Handle(new CreateSmartLinkCommand(SmartLinkType.Artist, artist.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().HaveLength(8);
        result.Url.Should().Be($"https://yuviron.com/artist/{artist.PublicId}?si={result.Code}");

        var linkInDb = await dbContext.Set<SmartLink>().FirstOrDefaultAsync(sl => sl.Code == result.Code);
        linkInDb.Should().NotBeNull();
        linkInDb!.CreatedByUserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_Should_CreateAnonymousSmartLink_When_UserNotLoggedIn()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var artist = Artist.Create(null, "Test Artist", null, null, null, VerificationStatus.None, DateTime.UtcNow);
        dbContext.Add(artist);
        await dbContext.SaveChangesAsync();

        var currentUser = Mock.Of<ICurrentUserService>(s => s.UserId == (Guid?)null);
        
        var handler = new CreateSmartLinkHandler(
            dbContext, dbContext, dbContext, 
            dbContext,
            currentUser, TimeProvider.System, _frontendOptions);

        var result = await handler.Handle(new CreateSmartLinkCommand(SmartLinkType.Artist, artist.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Url.Should().Be($"https://yuviron.com/artist/{artist.PublicId}?si={result.Code}");
        var linkInDb = await dbContext.Set<SmartLink>().FirstOrDefaultAsync(sl => sl.Code == result.Code);
        linkInDb.Should().NotBeNull();
        linkInDb!.CreatedByUserId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnExistingLink_When_AlreadyCreatedBySameUser()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;
        
        var user = User.Create("test@example.com", "hash", "Test", false, true, utcNow);
        var artist = Artist.Create(null, "Test Artist", null, null, null, VerificationStatus.None, utcNow);
        dbContext.AddRange(user, artist);
        
        var existingLink = SmartLink.Create("existing", SmartLinkType.Artist, artist.Id, user.Id, null, utcNow);
        dbContext.Add(existingLink);
        
        await dbContext.SaveChangesAsync();

        var currentUser = Mock.Of<ICurrentUserService>(s => s.UserId == user.Id);
        
        var handler = new CreateSmartLinkHandler(
            dbContext, dbContext, dbContext, 
            dbContext,
            currentUser, TimeProvider.System, _frontendOptions);

        var result = await handler.Handle(new CreateSmartLinkCommand(SmartLinkType.Artist, artist.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be("existing");
        result.Url.Should().Be($"https://yuviron.com/artist/{artist.PublicId}?si=existing");
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_When_EntityDoesNotExist()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var currentUser = Mock.Of<ICurrentUserService>(s => s.UserId == Guid.NewGuid());
        
        var handler = new CreateSmartLinkHandler(
            dbContext, dbContext, dbContext, 
            dbContext,
            currentUser, TimeProvider.System, _frontendOptions);

        Func<Task> action = () => handler.Handle(new CreateSmartLinkCommand(SmartLinkType.Track, Guid.NewGuid()), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_CreateUserProfileSmartLink()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;

        var user = User.Create("profile@example.com", "hash", "Profile", false, true, utcNow);
        var profile = UserProfile.Create(user.Id, "Profile", null, null, null, null, null, utcNow, Gender.Male, utcNow);
        user.SetProfile(profile);
        dbContext.Add(user);
        await dbContext.SaveChangesAsync();

        var currentUser = Mock.Of<ICurrentUserService>(s => s.UserId == user.Id);

        var handler = new CreateSmartLinkHandler(
            dbContext, dbContext, dbContext,
            dbContext,
            currentUser, TimeProvider.System, _frontendOptions);

        var result = await handler.Handle(new CreateSmartLinkCommand(SmartLinkType.UserProfile, user.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().HaveLength(8);
        result.Url.Should().Be($"https://yuviron.com/user/{profile.PublicId}?si={result.Code}");

        var linkInDb = await dbContext.Set<SmartLink>().FirstOrDefaultAsync(sl => sl.Code == result.Code);
        linkInDb.Should().NotBeNull();
        linkInDb!.EntityType.Should().Be(SmartLinkType.UserProfile);
        linkInDb.EntityId.Should().Be(user.Id);
    }
}
