using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Complaints.Commands.CreateComplaint;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Complaints;

public class CreateComplaintHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Complaint_And_Increment_Counter()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var currentUserMock = new Mock<ICurrentUserService>();
        var userId = Guid.NewGuid();
        currentUserMock.Setup(x => x.UserId).Returns(userId);

        var utcNow = DateTime.UtcNow;
        var artist = Artist.Create(null, "Artist", null, null, null, VerificationStatus.None, utcNow);
        dbContext.Artists.Add(artist);
        dbContext.Users.Add(User.Create("user@mail.com", "hash", "User", true, true, utcNow));
        await dbContext.SaveChangesAsync();

        var handler = new CreateComplaintHandler(dbContext, currentUserMock.Object, TimeProvider.System);

        var command = new CreateComplaintCommand(ComplaintTargetType.Artist, artist.Id, "spam", "Looks bad");

        var complaintId = await handler.Handle(command, CancellationToken.None);

        var complaint = await dbContext.Complaints.FindAsync(complaintId);
        complaint.Should().NotBeNull();
        complaint!.CreatedByUserId.Should().Be(userId);
        complaint.Status.Should().Be(ComplaintStatus.New);

        var counter = await dbContext.ComplaintCounters.FindAsync(ComplaintTargetType.Artist, artist.Id);
        counter.Should().NotBeNull();
        counter!.CountOpen.Should().Be(1);
        counter.CountTotal.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Create_Complaint_For_User_Target()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var currentUserMock = new Mock<ICurrentUserService>();
        var userId = Guid.NewGuid();
        currentUserMock.Setup(x => x.UserId).Returns(userId);

        var utcNow = DateTime.UtcNow;
        dbContext.Users.Add(User.Create("reporter@mail.com", "hash", "Reporter", true, true, utcNow));
        var targetUser = User.Create("target@mail.com", "hash", "Target", true, true, utcNow);
        dbContext.Users.Add(targetUser);
        dbContext.UserProfiles.Add(UserProfile.Create(targetUser.Id, "Target", null, null, null, null, null, utcNow, Gender.Male, utcNow));
        await dbContext.SaveChangesAsync();

        var handler = new CreateComplaintHandler(dbContext, currentUserMock.Object, TimeProvider.System);

        var command = new CreateComplaintCommand(ComplaintTargetType.User, targetUser.Id, "abuse", "Offensive profile");

        var complaintId = await handler.Handle(command, CancellationToken.None);

        var complaint = await dbContext.Complaints.FindAsync(complaintId);
        complaint.Should().NotBeNull();
        complaint!.TargetType.Should().Be(ComplaintTargetType.User);
        complaint.TargetId.Should().Be(targetUser.Id);
    }
}
