using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.VerificationRequests.Commands.RejectRequest;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Admin.VerificationRequests;

public class RejectVerificationRequestHandlerTests
{
    [Fact]
    public async Task Handle_Should_ChangeStatusToRejected()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new AppDbContext(options);
        
        var adminId = Guid.NewGuid();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns(adminId);

        var utcNow = DateTime.UtcNow;
        var artist = Artist.Create(null, "Test Band", null, null, null, VerificationStatus.None, utcNow);
        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        
        dbContext.Artists.Add(artist);
        dbContext.Users.Add(user);
        
        var request = VerificationRequest.Create(
            artist.Id, user.Id, ClaimRole.Manager, "bad@mail.com", "bad-link", null, null, utcNow);
        dbContext.VerificationRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var handler = new RejectVerificationRequestHandler(dbContext, TimeProvider.System, currentUserServiceMock.Object);
        var command = new RejectVerificationRequestCommand(request.Id, "Fake links provided");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedReq = await dbContext.VerificationRequests.FindAsync(request.Id);
        updatedReq!.Status.Should().Be(VerificationRequestStatus.Rejected);
        updatedReq.AdminId.Should().Be(adminId);
        updatedReq.AdminNote.Should().Be("Fake links provided");
    }
    
    [Fact]
    public async Task Handle_Should_ThrowException_When_RequestIsNotPending()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new AppDbContext(options);
        
        var adminId = Guid.NewGuid();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns(adminId);

        var utcNow = DateTime.UtcNow;
        var artist = Artist.Create(null, "Test Band", null, null, null, VerificationStatus.None, utcNow);
        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        
        dbContext.Artists.Add(artist);
        dbContext.Users.Add(user);
        
        var request = VerificationRequest.Create(
            artist.Id, user.Id, ClaimRole.Manager, "bad@mail.com", "bad-link", null, null, utcNow);
        
        // Моделируем ситуацию: заявку уже одобрили
        request.Approve(adminId, "Approved by someone else", utcNow); 
        dbContext.VerificationRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var handler = new RejectVerificationRequestHandler(dbContext, TimeProvider.System, currentUserServiceMock.Object);
        var command = new RejectVerificationRequestCommand(request.Id, "Trying to reject");

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only pending requests can be rejected.");
    }
}