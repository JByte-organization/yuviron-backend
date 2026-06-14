using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.VerificationRequests.Commands.RejectRequest;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Admin.VerificationRequests;

public class RejectVerificationRequestHandlerTests
{
    [Fact]
    public async Task Handle_Should_ChangeStatusToRejected_And_PublishEvent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new AppDbContext(options);
        
        var adminId = Guid.NewGuid();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns(adminId);
        
        // ДОБАВИЛИ МОК ДЛЯ IEventBus
        var eventBusMock = new Mock<IEventBus>(); 

        var utcNow = DateTime.UtcNow;
        var artist = Artist.Create(null, "Test Band", null, null, null, VerificationStatus.None, utcNow);
        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        
        dbContext.Add(artist);
        dbContext.Add(user);
        
        var request = VerificationRequest.Create(
            artist.Id, user.Id, ClaimRole.Manager, "bad@mail.com", "bad-link", null, null, utcNow);
        dbContext.Add(request);
        await dbContext.SaveChangesAsync();

        // ПЕРЕДАЛИ eventBusMock.Object В КОНСТРУКТОР
        var handler = new RejectVerificationRequestHandler(
            dbContext, TimeProvider.System, currentUserServiceMock.Object, eventBusMock.Object);
            
        var command = new RejectVerificationRequestCommand(request.Id, "Fake links provided");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedReq = await dbContext.Set<VerificationRequest>().FindAsync(request.Id);
        updatedReq!.Status.Should().Be(VerificationRequestStatus.Rejected);
        updatedReq.AdminId.Should().Be(adminId);
        updatedReq.AdminNote.Should().Be("Fake links provided");
        
        // ПРОВЕРЯЕМ, ЧТО ИВЕНТ БЫЛ ОТПРАВЛЕН
        eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ArtistClaimRejectedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
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
        
        // ДОБАВИЛИ МОК ДЛЯ IEventBus
        var eventBusMock = new Mock<IEventBus>(); 

        var utcNow = DateTime.UtcNow;
        var artist = Artist.Create(null, "Test Band", null, null, null, VerificationStatus.None, utcNow);
        var user = User.Create("test@mail.com", "hash", "Test", true, true, utcNow);
        
        dbContext.Add(artist);
        dbContext.Add(user);
        
        var request = VerificationRequest.Create(
            artist.Id, user.Id, ClaimRole.Manager, "bad@mail.com", "bad-link", null, null, utcNow);
        
        request.Approve(adminId, "Approved by someone else", utcNow); 
        dbContext.Add(request);
        await dbContext.SaveChangesAsync();

        // ПЕРЕДАЛИ eventBusMock.Object В КОНСТРУКТОР
        var handler = new RejectVerificationRequestHandler(
            dbContext, TimeProvider.System, currentUserServiceMock.Object, eventBusMock.Object);
            
        var command = new RejectVerificationRequestCommand(request.Id, "Trying to reject");

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only pending requests can be rejected.");
            
        // ПРОВЕРЯЕМ, ЧТО ИВЕНТ НЕ ОТПРАВЛЯЛСЯ ИЗ-ЗА ОШИБКИ
        eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ArtistClaimRejectedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}