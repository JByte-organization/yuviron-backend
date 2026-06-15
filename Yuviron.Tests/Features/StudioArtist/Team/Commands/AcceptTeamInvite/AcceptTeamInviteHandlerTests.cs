using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.StudioArtist.Team.Commands.AcceptTeamInvite;
using Yuviron.Application.Features.StudioArtist.Team.Commands.AddTeamMember;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Team.Commands.AcceptTeamInvite;

public class AcceptTeamInviteHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Handle_Should_AddMember_When_TokenIsValid()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;
        var user = User.Create("invitee@example.com", "hash", "Invitee", false, true, utcNow);
        var artist = Artist.Create(null, "The Artist", null, null, null, VerificationStatus.None, utcNow);
        dbContext.AddRange(user, artist);
        await dbContext.SaveChangesAsync();

        var token = "valid-token";
        var inviteData = new TeamInvitationData(artist.Id, user.Email, ArtistTeamRole.Manager);

        var cacheMock = new Mock<ICacheService>();
        cacheMock.Setup(x => x.GetAsync<TeamInvitationData>($"team_invite:{token}", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(inviteData);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(user.Id);

        var eventBusMock = new Mock<IEventBus>();

        var handler = new AcceptTeamInviteHandler(
            dbContext, dbContext, currentUserMock.Object, cacheMock.Object, TimeProvider.System, eventBusMock.Object);

        await handler.Handle(new AcceptTeamInviteCommand(token), CancellationToken.None);

        var updatedArtist = await dbContext.Artists.Include(a => a.TeamMembers).FirstAsync(a => a.Id == artist.Id);
        updatedArtist.TeamMembers.Should().Contain(tm => tm.UserId == user.Id && tm.Role == ArtistTeamRole.Manager);
    }

    [Fact]
    public async Task Handle_Should_ThrowForbidden_When_EmailMismatched()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;
        var user = User.Create("wrong@example.com", "hash", "Wrong", false, true, utcNow);
        var artist = Artist.Create(null, "The Artist", null, null, null, VerificationStatus.None, utcNow);
        dbContext.AddRange(user, artist);
        await dbContext.SaveChangesAsync();

        var token = "valid-token";
        var inviteData = new TeamInvitationData(artist.Id, "target@example.com", ArtistTeamRole.Manager);

        var cacheMock = new Mock<ICacheService>();
        cacheMock.Setup(x => x.GetAsync<TeamInvitationData>($"team_invite:{token}", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(inviteData);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(user.Id);

        var handler = new AcceptTeamInviteHandler(
            dbContext, dbContext, currentUserMock.Object, cacheMock.Object, TimeProvider.System, Mock.Of<IEventBus>());

        Func<Task> action = () => handler.Handle(new AcceptTeamInviteCommand(token), CancellationToken.None);

        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("This invitation was sent to a different email address.");
    }
}
