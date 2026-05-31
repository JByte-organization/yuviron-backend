using MassTransit;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers;

namespace Yuviron.Tests.Infrastructure.Consumers;

public class CreateArtistClaimApprovedNotificationConsumerTests
{
    [Fact]
    public async Task Consume_Should_CallNotificationService_WithCorrectParameters()
    {
        var userId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var artistName = "Linkin Park";
        var eventMessage = new ArtistClaimApprovedEvent(userId, artistId, artistName);

        var consumeContextMock = new Mock<ConsumeContext<ArtistClaimApprovedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(eventMessage);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var notificationServiceMock = new Mock<INotificationService>();

        var consumer = new CreateArtistClaimApprovedNotificationConsumer(notificationServiceMock.Object);

        await consumer.Consume(consumeContextMock.Object);

        notificationServiceMock.Verify(x => x.SendToUserAsync(
            userId,
            "Заявка на профіль схвалена! 🎵",
            $"Вітаємо! Ваш профіль артиста «{artistName}» успішно підтверджено. Тепер ви маєте доступ до Студії.",
            NotificationEntityType.Artist,
            artistId,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}