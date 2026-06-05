using MassTransit;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers;

namespace Yuviron.Tests.Infrastructure.Consumers;

public class NotifyUserOnTeamMemberRemovedConsumerTests
{
    [Fact]
    public async Task Consume_Should_CallNotificationService_WithCorrectParameters()
    {
        var userId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var artistName = "Test Artist";
        var eventMessage = new TeamMemberRemovedEvent(userId, artistId, artistName);

        var consumeContextMock = new Mock<ConsumeContext<TeamMemberRemovedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(eventMessage);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var notificationServiceMock = new Mock<INotificationService>();
        var consumer = new NotifyUserOnTeamMemberRemovedConsumer(notificationServiceMock.Object);

        await consumer.Consume(consumeContextMock.Object);

        notificationServiceMock.Verify(x => x.SendToUserAsync(
            userId,
            NotificationCategory.System,
            "team_member_removed",
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains(artistName)),
            NotificationEntityType.Artist,
            artistId,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
