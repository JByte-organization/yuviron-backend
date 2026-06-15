using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.StudioArtist.Team.Commands.UpdateTeamMemberRole;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.StudioArtist.Team.Commands.UpdateTeamMemberRole;

public class UpdateTeamMemberRoleHandlerTests
{
    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Handle_Should_TransferOwnership_When_PromotingToOwner()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;

        var owner = User.Create("owner@example.com", "hash", "Owner", false, true, utcNow);
        var editor = User.Create("editor@example.com", "hash", "Editor", false, true, utcNow);
        dbContext.AddRange(owner, editor);

        var artist = Artist.Create(owner.Id, "The Artist", null, null, null, VerificationStatus.None, utcNow);
        artist.AddTeamMember(editor.Id, ArtistTeamRole.Editor, utcNow);
        dbContext.Add(artist);
        await dbContext.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(owner.Id);

        var eventBusMock = new Mock<IEventBus>();

        var handler = new UpdateTeamMemberRoleHandler(
            dbContext, currentUserMock.Object, TimeProvider.System, eventBusMock.Object);

        // Act
        await handler.Handle(new UpdateTeamMemberRoleCommand(artist.Id, editor.Id, ArtistTeamRole.Owner), CancellationToken.None);

        // Assert
        var updatedArtist = await dbContext.Artists.Include(a => a.TeamMembers).FirstAsync(a => a.Id == artist.Id);
        var oldOwner = updatedArtist.TeamMembers.First(tm => tm.UserId == owner.Id);
        var newOwner = updatedArtist.TeamMembers.First(tm => tm.UserId == editor.Id);

        oldOwner.Role.Should().Be(ArtistTeamRole.Manager);
        newOwner.Role.Should().Be(ArtistTeamRole.Owner);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_OwnerDemotesThemselves_WithoutSuccessor()
    {
        await using var dbContext = new AppDbContext(CreateOptions());
        var utcNow = DateTime.UtcNow;

        var owner = User.Create("owner@example.com", "hash", "Owner", false, true, utcNow);
        dbContext.Add(owner);

        var artist = Artist.Create(owner.Id, "The Artist", null, null, null, VerificationStatus.None, utcNow);
        dbContext.Add(artist);
        await dbContext.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(owner.Id);

        var eventBusMock = new Mock<IEventBus>();

        var handler = new UpdateTeamMemberRoleHandler(
            dbContext, currentUserMock.Object, TimeProvider.System, eventBusMock.Object);

        // Act
        Func<Task> action = () => handler.Handle(new UpdateTeamMemberRoleCommand(artist.Id, owner.Id, ArtistTeamRole.Editor), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot demote the only owner. Transfer ownership to another member first.");
    }
}
