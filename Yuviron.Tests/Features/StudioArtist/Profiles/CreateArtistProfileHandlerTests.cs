using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.CreateProfile;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Profiles;

public class CreateArtistProfileHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;

    public CreateArtistProfileHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _permissionServiceMock = new Mock<IPermissionService>();
    }

    [Fact]
    public async Task Handle_Should_CreateArtist_And_AssignManagementRole()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        // 1. Создаем юзера честно
        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        dbContext.Add(user);

        // 2. Учим мок возвращать сгенерированный ID этого юзера
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        var managementRole = Role.Create(nameof(RoleName.ManagementUser));
        dbContext.Add(managementRole);
        await dbContext.SaveChangesAsync();

        var options = Options.Create(new ArtistLimitsOptions { FreeUserMaxProfiles = 1, PremiumUserMaxProfiles = 5 });
        
        var handler = new CreateArtistProfileHandler(dbContext, dbContext, dbContext, TimeProvider.System, options, _currentUserServiceMock.Object, _permissionServiceMock.Object);
            
        var command = new CreateArtistProfileCommand("My New Band", null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty(); 
        
        var artist = await dbContext.Artists.Include(a => a.TeamMembers).FirstAsync(a => a.Id == result);
        artist.Name.Should().Be("My New Band");
        artist.VerificationStatus.Should().Be(VerificationStatus.None); 
        artist.TeamMembers.Should().ContainSingle(tm => tm.UserId == user.Id && tm.Role == ArtistTeamRole.Owner);

        var updatedUser = await dbContext.Users.Include(u => u.UserRoles).FirstAsync(u => u.Id == user.Id);
        updatedUser.UserRoles.Should().ContainSingle(ur => ur.RoleId == managementRole.Id); 

        _permissionServiceMock.Verify(x => x.InvalidatePermissionsAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowForbidden_When_LimitExceeded()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var utcNow = DateTime.UtcNow;

        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        dbContext.Add(user);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        var options = Options.Create(new ArtistLimitsOptions { FreeUserMaxProfiles = 1, PremiumUserMaxProfiles = 5 });
        
        var existingArtist = Artist.Create(user.Id, "Old Band", null, null, null, VerificationStatus.None, utcNow);
        dbContext.Add(existingArtist);
        await dbContext.SaveChangesAsync();

        var handler = new CreateArtistProfileHandler(dbContext, dbContext, dbContext, TimeProvider.System, options, _currentUserServiceMock.Object, _permissionServiceMock.Object);
            
        var command = new CreateArtistProfileCommand("Second Band", null);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);
        
        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*Limit exceeded*"); 
    }
}