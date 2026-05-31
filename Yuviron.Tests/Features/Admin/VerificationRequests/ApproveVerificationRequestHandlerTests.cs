using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.VerificationRequests.Commands.ApproveRequest;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Admin.VerificationRequests;

public class ApproveVerificationRequestHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<IEventBus> _eventBusMock; 

    public ApproveVerificationRequestHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _permissionServiceMock = new Mock<IPermissionService>();
        _eventBusMock = new Mock<IEventBus>(); 
    }

    [Fact]
    public async Task Handle_Should_Approve_GrantRoles_GiveVerifiedBadge_And_RejectCompetitors()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var adminId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(adminId);

        var utcNow = DateTime.UtcNow;

        var managementRole = Role.Create(nameof(RoleName.ManagementUser));
        dbContext.Roles.Add(managementRole);

        var artist = Artist.Create(null, "Eminem", null, null, null, VerificationStatus.None, utcNow);
        dbContext.Artists.Add(artist);

        var realUser = User.Create("real@mail.com", "hash", "Real", true, true, utcNow);
        dbContext.Users.Add(realUser);

        var fakeUser = User.Create("fake@mail.com", "hash", "Fake", true, true, utcNow);
        dbContext.Users.Add(fakeUser);

        var realRequest = VerificationRequest.Create(
            artist.Id, realUser.Id, ClaimRole.Artist, "official@eminem.com", "link", null, null, utcNow);
        dbContext.VerificationRequests.Add(realRequest);

        var fakeRequest = VerificationRequest.Create(
            artist.Id, fakeUser.Id, ClaimRole.Artist, "scammer@mail.com", "link", null, null, utcNow);
        dbContext.VerificationRequests.Add(fakeRequest);

        await dbContext.SaveChangesAsync();

        var handler = new ApproveVerificationRequestHandler(
            dbContext, TimeProvider.System, _currentUserServiceMock.Object, _permissionServiceMock.Object, _eventBusMock.Object); 
            
        var command = new ApproveVerificationRequestCommand(realRequest.Id, "All good");

        await handler.Handle(command, CancellationToken.None);

        var updatedRealReq = await dbContext.VerificationRequests.FindAsync(realRequest.Id);
        updatedRealReq!.Status.Should().Be(VerificationRequestStatus.Approved);

        var updatedFakeReq = await dbContext.VerificationRequests.FindAsync(fakeRequest.Id);
        updatedFakeReq!.Status.Should().Be(VerificationRequestStatus.Rejected);
        
        var updatedArtist = await dbContext.Artists.Include(a => a.TeamMembers).FirstAsync(a => a.Id == artist.Id);
        updatedArtist.VerificationStatus.Should().Be(VerificationStatus.Verified);
        
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ArtistClaimApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_Should_ThrowException_When_RequestIsNotPending()
    {
        var dbContext = new AppDbContext(_dbOptions);
        var adminId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(adminId);

        var utcNow = DateTime.UtcNow;
        var artist = Artist.Create(null, "Band", null, null, null, VerificationStatus.None, utcNow);
        var user = User.Create("mail@mail.com", "hash", "User", true, true, utcNow);
        
        dbContext.Artists.Add(artist);
        dbContext.Users.Add(user);

        var request = VerificationRequest.Create(artist.Id, user.Id, ClaimRole.Artist, "m@m.com", "link", null, null, utcNow);
        
        request.Reject(adminId, "Declined", utcNow); 
        dbContext.VerificationRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var handler = new ApproveVerificationRequestHandler(
            dbContext, TimeProvider.System, _currentUserServiceMock.Object, _permissionServiceMock.Object, _eventBusMock.Object); 
            
        var command = new ApproveVerificationRequestCommand(request.Id, "Trying to approve again");

        var action = async () => await handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
        
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ArtistClaimApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}