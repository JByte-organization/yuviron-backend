using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Notifications.Queries.GetNotifications;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Notifications;

public class GetNotificationsHandlerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetNotificationsHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_Should_ReturnOnlyUserNotifications()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var utcNow = DateTime.UtcNow;

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        // 1. Уведомление текущего юзера (Трек)
        dbContext.Notifications.Add(Notification.Create(currentUserId, "New Track", "Body", NotificationEntityType.Track, Guid.NewGuid(), utcNow));
        
        // 2. Уведомление текущего юзера (Система)
        dbContext.Notifications.Add(Notification.Create(currentUserId, "Welcome", "Body", NotificationEntityType.System, null, utcNow.AddDays(-1)));

        // 3. Чужое уведомление (не должно попасть в выдачу)
        dbContext.Notifications.Add(Notification.Create(otherUserId, "Other", "Body", NotificationEntityType.Track, null, utcNow));

        await dbContext.SaveChangesAsync();

        var handler = new GetNotificationsHandler(dbContext, _currentUserServiceMock.Object);
        var query = new GetNotificationsQuery(); // Запрашиваем всё, без фильтров

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        // Обращаемся напрямую к свойствам PaginatedList
        result.TotalCount.Should().Be(2); 
        result.Items.Should().Contain(n => n.Title == "New Track");
        result.Items.Should().Contain(n => n.Title == "Welcome");
        result.Items.Should().NotContain(n => n.Title == "Other"); 
    }

    [Fact]
    public async Task Handle_Should_FilterByEntityTypes()
    {
        // Arrange
        var dbContext = new AppDbContext(_dbOptions);
        var currentUserId = Guid.NewGuid();
        var utcNow = DateTime.UtcNow;

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        dbContext.Notifications.Add(Notification.Create(currentUserId, "Track 1", "Body", NotificationEntityType.Track, null, utcNow));
        dbContext.Notifications.Add(Notification.Create(currentUserId, "Album 1", "Body", NotificationEntityType.Album, null, utcNow));
        dbContext.Notifications.Add(Notification.Create(currentUserId, "System", "Body", NotificationEntityType.System, null, utcNow));

        await dbContext.SaveChangesAsync();

        var handler = new GetNotificationsHandler(dbContext, _currentUserServiceMock.Object);
        
        var query = new GetNotificationsQuery(Types: new List<NotificationEntityType> 
        { 
            NotificationEntityType.Track, 
            NotificationEntityType.Album 
        });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2); // Системное должно отсеяться
        result.Items.Should().OnlyContain(n => n.EntityType == "Track" || n.EntityType == "Album");
    }
}