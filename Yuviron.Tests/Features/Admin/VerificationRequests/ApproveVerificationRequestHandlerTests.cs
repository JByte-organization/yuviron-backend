using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.VerificationRequests.Commands.ApproveRequest;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Admin.VerificationRequests;

public class ApproveVerificationRequestHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;

    public ApproveVerificationRequestHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _permissionServiceMock = new Mock<IPermissionService>();
    }

    [Fact]
    public async Task Handle_Should_Approve_GrantRoles_GiveVerifiedBadge_And_RejectCompetitors()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var adminId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(adminId);

        var utcNow = DateTime.UtcNow;

        var managementRole = Role.Create(nameof(RoleName.ManagementUser));
        dbContext.Roles.Add(managementRole);

        // Ничейный артист без галочки
        var artist = Artist.Create(null, "Eminem", null, null, null, VerificationStatus.None, utcNow);
        dbContext.Artists.Add(artist);

        // Настоящий юзер (Вася)
        var realUser = User.Create("real@mail.com", "hash", "Real", true, true, utcNow);
        dbContext.Users.Add(realUser);

        // Фейковый юзер (Петя)
        var fakeUser = User.Create("fake@mail.com", "hash", "Fake", true, true, utcNow);
        dbContext.Users.Add(fakeUser);

        // Заявка Васи (её мы одобрим)
        var realRequest = VerificationRequest.Create(
            artist.Id, realUser.Id, ClaimRole.Artist, "official@eminem.com", "link", null, null, utcNow);
        dbContext.VerificationRequests.Add(realRequest);

        // Заявка Пети (мошенник)
        var fakeRequest = VerificationRequest.Create(
            artist.Id, fakeUser.Id, ClaimRole.Artist, "scammer@mail.com", "link", null, null, utcNow);
        dbContext.VerificationRequests.Add(fakeRequest);

        await dbContext.SaveChangesAsync();

        var handler = new ApproveVerificationRequestHandler(
            dbContext, TimeProvider.System, _currentUserServiceMock.Object, _permissionServiceMock.Object);
            
        var command = new ApproveVerificationRequestCommand(realRequest.Id, "All good");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        
        // 1. Заявка Васи одобрена
        var updatedRealReq = await dbContext.VerificationRequests.FindAsync(realRequest.Id);
        updatedRealReq!.Status.Should().Be(VerificationRequestStatus.Approved);

        // 2. Заявка Пети отклонена (конкурент)
        var updatedFakeReq = await dbContext.VerificationRequests.FindAsync(fakeRequest.Id);
        updatedFakeReq!.Status.Should().Be(VerificationRequestStatus.Rejected);
        updatedFakeReq.AdminNote.Should().Contain("verified by another user");

        // 3. Артист получил галочку и владельца
        var updatedArtist = await dbContext.Artists.Include(a => a.TeamMembers).FirstAsync(a => a.Id == artist.Id);
        updatedArtist.VerificationStatus.Should().Be(VerificationStatus.Verified);
        updatedArtist.TeamMembers.Should().ContainSingle(tm => tm.UserId == realUser.Id && tm.Role == ArtistTeamRole.Owner);

        // 4. Вася получил права в Студию
        var updatedRealUser = await dbContext.Users.Include(u => u.UserRoles).FirstAsync(u => u.Id == realUser.Id);
        updatedRealUser.UserRoles.Should().ContainSingle(ur => ur.RoleId == managementRole.Id);

        // 5. Кэш сброшен
        _permissionServiceMock.Verify(x => x.InvalidatePermissionsAsync(realUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_Should_ThrowException_When_RequestIsNotPending()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var adminId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(adminId);

        var utcNow = DateTime.UtcNow;
        var artist = Artist.Create(null, "Band", null, null, null, VerificationStatus.None, utcNow);
        var user = User.Create("mail@mail.com", "hash", "User", true, true, utcNow);
        
        dbContext.Artists.Add(artist);
        dbContext.Users.Add(user);

        var request = VerificationRequest.Create(artist.Id, user.Id, ClaimRole.Artist, "m@m.com", "link", null, null, utcNow);
        
        // ИМИТИРУЕМ, ЧТО ЗАЯВКА УЖЕ БЫЛА ОТКЛОНЕНА РАНЕЕ
        request.Reject(adminId, "Declined", utcNow); 
        dbContext.VerificationRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var handler = new ApproveVerificationRequestHandler(
            dbContext, TimeProvider.System, _currentUserServiceMock.Object, _permissionServiceMock.Object);
            
        var command = new ApproveVerificationRequestCommand(request.Id, "Trying to approve again");

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only pending requests can be approved.");
    }
}