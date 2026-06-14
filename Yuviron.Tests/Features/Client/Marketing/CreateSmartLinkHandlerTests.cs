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
        var userId = Guid.NewGuid();
        var artist = Artist.Create(null, "Test Artist", null, null, null, VerificationStatus.None, DateTime.UtcNow);
        dbContext.Add(artist);
        await dbContext.SaveChangesAsync();

        var currentUser = Mock.Of<ICurrentUserService>(s => s.UserId == userId);
        
        var handler = new CreateSmartLinkHandler(
            dbContext, dbContext, dbContext, 
            currentUser, TimeProvider.System, _frontendOptions);

        var result = await handler.Handle(new CreateSmartLinkCommand(SmartLinkType.Artist, artist.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().HaveLength(8);
        result.Url.Should().Be($"https://yuviron.com/sl/{result.Code}");

        var linkInDb = await dbContext.Set<SmartLink>().FirstOrDefaultAsync(sl => sl.Code == result.Code);
        linkInDb.Should().NotBeNull();
        linkInDb!.EntityId.Should().Be(artist.Id);
        linkInDb.EntityType.Should().Be(SmartLinkType.Artist);
        linkInDb.CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_Should_ReturnExistingLink_When_AlreadyCreatedBySameUser()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        var artist = Artist.Create(null, "Test Artist", null, null, null, VerificationStatus.None, DateTime.UtcNow);
        dbContext.Add(artist);
        
        var existingLink = SmartLink.Create("existing", SmartLinkType.Artist, artist.Id, userId, null, DateTime.UtcNow);
        dbContext.Add(existingLink);
        
        await dbContext.SaveChangesAsync();

        var currentUser = Mock.Of<ICurrentUserService>(s => s.UserId == userId);
        
        var handler = new CreateSmartLinkHandler(
            dbContext, dbContext, dbContext, 
            currentUser, TimeProvider.System, _frontendOptions);

        var result = await handler.Handle(new CreateSmartLinkCommand(SmartLinkType.Artist, artist.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be("existing");
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_When_EntityDoesNotExist()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var userId = Guid.NewGuid();
        var currentUser = Mock.Of<ICurrentUserService>(s => s.UserId == userId);
        
        var handler = new CreateSmartLinkHandler(
            dbContext, dbContext, dbContext, 
            currentUser, TimeProvider.System, _frontendOptions);

        Func<Task> action = () => handler.Handle(new CreateSmartLinkCommand(SmartLinkType.Track, Guid.NewGuid()), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}

