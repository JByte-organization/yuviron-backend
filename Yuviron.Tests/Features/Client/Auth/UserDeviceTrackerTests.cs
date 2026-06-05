using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Yuviron.Application.Abstractions.Identity;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Entities;
using Yuviron.Infrastructure.Persistence;
using Yuviron.Infrastructure.Services;

namespace Yuviron.Tests.Infrastructure.Services;

public class UserDeviceTrackerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly UserDeviceTracker _tracker;

    public UserDeviceTrackerTests()
    {
        // 1. Настройка In-Memory базы
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        
        // 2. Создаем Mock для IEventBus
        _mockEventBus = new Mock<IEventBus>();
        
        // 3. Передаем обе зависимости в конструктор
        _tracker = new UserDeviceTracker(_context, _mockEventBus.Object);
    }

    [Fact]
    public async Task TrackDeviceAsync_ShouldCreateNewDevice_WhenDeviceIsNew()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fingerprint = "device-fingerprint-123";
        var clientInfo = new ClientContext("Windows 10", "Chrome", "192.168.1.1", fingerprint);

        // Act
        await _tracker.TrackDeviceAsync(userId, clientInfo, DateTime.UtcNow, CancellationToken.None);

        // Assert
        var device = await _context.UserDevices.FirstOrDefaultAsync(d => d.UserId == userId);
        device.Should().NotBeNull();
        device!.DeviceName.Should().Be("Windows 10");
        device.Fingerprint.Should().Be(fingerprint);
        
        _mockEventBus.Verify(bus => bus.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrackDeviceAsync_ShouldNotPublishEvent_WhenDeviceExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fingerprint = "device-fingerprint-123";
        var clientInfo = new ClientContext("Windows 10", "Chrome", "192.168.1.1", fingerprint);
        
        var existingDevice = UserDevice.Create(
            userId, 
            clientInfo.Fingerprint, 
            clientInfo.Device, 
            clientInfo.Browser, 
            clientInfo.IpAddress, 
            DateTime.UtcNow);

        _context.UserDevices.Add(existingDevice);
        await _context.SaveChangesAsync();

        // Act
        await _tracker.TrackDeviceAsync(userId, clientInfo, DateTime.UtcNow, CancellationToken.None);

        // Assert
        _mockEventBus.Verify(bus => bus.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}