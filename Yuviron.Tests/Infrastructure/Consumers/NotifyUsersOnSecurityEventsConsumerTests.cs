using Yuviron.Infrastructure.Consumers.Notifications.Email;
using Yuviron.Infrastructure.Consumers.Notifications.InApp;
﻿using MassTransit;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers;

namespace Yuviron.Tests.Infrastructure.Consumers;

public class NotifyUsersOnSecurityEventsConsumerTests
{
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly NotifyUsersOnSecurityEventsConsumer _consumer;

    public NotifyUsersOnSecurityEventsConsumerTests()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _consumer = new NotifyUsersOnSecurityEventsConsumer(_notificationServiceMock.Object);
    }

    [Fact]
    public async Task Consume_Should_Notify_User_About_New_Login()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var msg = new NewDeviceLoginEvent(userId, "iPhone 13", "Safari", "192.168.1.1");
        
        var consumeContextMock = new Mock<ConsumeContext<NewDeviceLoginEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(msg);
        consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _notificationServiceMock.Verify(x => x.SendToUserAsync(
            userId,
            NotificationCategory.System,
            "new_device_login",
            "Новий вхід в акаунт 🛡️",
            "Вхід з пристрою iPhone 13 (Safari). IP: 192.168.1.1.",
            NotificationEntityType.System,
            null,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
