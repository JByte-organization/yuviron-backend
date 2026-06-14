using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.Complaints.Commands.ApproveComplaint;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Admin.Complaints;

public class ApproveComplaintHandlerTests
{
    [Fact]
    public async Task Handle_Should_Approve_Complaint_Resolve_Counter_And_Publish_Event()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var adminId = Guid.NewGuid();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(adminId);
        var eventBusMock = new Mock<IEventBus>();

        var utcNow = DateTime.UtcNow;
        var artist = Artist.Create(null, "Artist", null, null, null, VerificationStatus.None, utcNow);
        var user = User.Create("user@mail.com", "hash", "User", true, true, utcNow);

        dbContext.Add(artist);
        dbContext.Add(user);
        var complaint = Complaint.Create(user.Id, ComplaintTargetType.Artist, artist.Id, ComplaintReasonCode.Spam, "bad content", utcNow);
        dbContext.Add(complaint);
        dbContext.Add(ComplaintCounter.Create(ComplaintTargetType.Artist, artist.Id, utcNow));
        await dbContext.SaveChangesAsync();

        var handler = new ApproveComplaintHandler(dbContext, dbContext, dbContext, currentUserMock.Object, TimeProvider.System, eventBusMock.Object);

        await handler.Handle(new ApproveComplaintCommand(complaint.Id, "Valid report"), CancellationToken.None);

        var updatedComplaint = await dbContext.Set<Complaint>().FindAsync(complaint.Id);
        updatedComplaint!.Status.Should().Be(ComplaintStatus.Approved);
        updatedComplaint.ModeratedByAdminId.Should().Be(adminId);
        updatedComplaint.ModerationNote.Should().Be("Valid report");

        var counter = await dbContext.Set<ComplaintCounter>().FindAsync(ComplaintTargetType.Artist, artist.Id);
        counter!.CountOpen.Should().Be(0);

        eventBusMock.Verify(x => x.PublishAsync(
            It.Is<ComplaintApprovedEvent>(e =>
                e.ComplaintId == complaint.Id &&
                e.UserId == user.Id &&
                e.TargetId == artist.Id &&
                e.TargetType == ComplaintTargetType.Artist),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Approve_User_Complaint_And_Resolve_User_Profile_Title()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var adminId = Guid.NewGuid();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(adminId);
        var eventBusMock = new Mock<IEventBus>();

        var utcNow = DateTime.UtcNow;
        var reporter = User.Create("reporter@mail.com", "hash", "Reporter", true, true, utcNow);
        var targetUser = User.Create("target@mail.com", "hash", "Target User", true, true, utcNow);
        dbContext.Add(reporter);
        dbContext.Add(targetUser);
        dbContext.Add(UserProfile.Create(targetUser.Id, "Target User", null, null, null, null, null, utcNow, Gender.Male, utcNow));
        dbContext.Add(Complaint.Create(reporter.Id, ComplaintTargetType.User, targetUser.Id, ComplaintReasonCode.Abuse, "bad behavior", utcNow));
        dbContext.Add(ComplaintCounter.Create(ComplaintTargetType.User, targetUser.Id, utcNow));
        await dbContext.SaveChangesAsync();

        var complaint = await dbContext.Complaints.FirstAsync();
        var handler = new ApproveComplaintHandler(dbContext, dbContext, dbContext, currentUserMock.Object, TimeProvider.System, eventBusMock.Object);

        await handler.Handle(new ApproveComplaintCommand(complaint.Id, "Confirmed"), CancellationToken.None);

        eventBusMock.Verify(x => x.PublishAsync(
            It.Is<ComplaintApprovedEvent>(e =>
                e.TargetType == ComplaintTargetType.User &&
                e.TargetId == targetUser.Id &&
                e.TargetTitle == "Target User"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
