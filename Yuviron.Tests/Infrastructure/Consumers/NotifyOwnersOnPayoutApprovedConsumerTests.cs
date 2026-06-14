using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Infrastructure.Consumers;

public class NotifyOwnersOnPayoutApprovedConsumerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly NotifyOwnersOnPayoutApprovedConsumer _consumer;

    public NotifyOwnersOnPayoutApprovedConsumerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new AppDbContext(options);
        _notificationServiceMock = new Mock<INotificationService>();
        _consumer = new NotifyOwnersOnPayoutApprovedConsumer(_context, _notificationServiceMock.Object);
    }

    [Fact]
    public async Task Consume_Should_Notify_All_Owners()
    {
        // Arrange
        var artist = Artist.Create(null, "Test Artist", null, null, null, VerificationStatus.None, DateTime.UtcNow); _context.Add(artist);
        var artistId = artist.Id;

        var owner1 = User.Create("owner1@test.com", "hash", "O1", true, true, DateTime.UtcNow);
        var owner2 = User.Create("owner2@test.com", "hash", "O2", true, true, DateTime.UtcNow);
        var member = User.Create("member@test.com", "hash", "M", true, true, DateTime.UtcNow); _context.AddRange(owner1, owner2, member);
        var owner1Id = owner1.Id;
        var owner2Id = owner2.Id;
        var memberId = member.Id; _context.Add(ArtistTeamMember.Create(artistId, owner1Id, ArtistTeamRole.Owner, DateTime.UtcNow)); _context.Add(ArtistTeamMember.Create(artistId, owner2Id, ArtistTeamRole.Owner, DateTime.UtcNow)); _context.Add(ArtistTeamMember.Create(artistId, memberId, ArtistTeamRole.Manager, DateTime.UtcNow));
        await _context.SaveChangesAsync();

        var msg = new PayoutApprovedEvent(artistId, 150.50m);
        var consumeContextMock = new Mock<ConsumeContext<PayoutApprovedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _notificationServiceMock.Verify(x => x.SendToUsersAsync(
            It.Is<IEnumerable<Guid>>(ids => ids.Contains(owner1Id) && ids.Contains(owner2Id) && !ids.Contains(memberId) && ids.Count() == 2),
            NotificationCategory.Billing,
            "payout_approved",
            It.IsAny<string>(),
            It.IsAny<string>(),
            NotificationEntityType.Artist,
            artistId,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_Not_Call_NotificationService_When_No_Owners()
    {
        // Arrange
        var artistId = Guid.NewGuid(); 
        
        var msg = new PayoutApprovedEvent(artistId, 100.00m);
        var consumeContextMock = new Mock<ConsumeContext<PayoutApprovedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _notificationServiceMock.Verify(x => x.SendToUsersAsync(
            It.IsAny<IEnumerable<Guid>>(),
            It.IsAny<NotificationCategory>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationEntityType>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }
}


