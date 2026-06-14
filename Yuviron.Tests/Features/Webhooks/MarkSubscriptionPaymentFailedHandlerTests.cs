using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Features.Webhooks.Commands.MarkSubscriptionPaymentFailed;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Webhooks;

public class MarkSubscriptionPaymentFailedHandlerTests
{
    [Fact]
    public async Task Handle_Should_Mark_User_Subscription_As_PastDue_And_Publish_Event()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new AppDbContext(options);
        var eventBusMock = new Mock<IEventBus>();
        var utcNow = DateTime.UtcNow;

        var user = User.Create("user@mail.com", "hash", "User", true, true, utcNow);
        var plan = Plan.Create("Premium", 9.99m, "USD", PlanPeriod.Month, PlanType.Listener, utcNow);
        dbContext.Add(user);
        dbContext.Add(plan);
        dbContext.Add(Subscription.Create(user.Id, plan.Id, utcNow.AddMonths(-1), utcNow.AddDays(10), SubscriptionStatus.Active, utcNow, "sub_123"));
        await dbContext.SaveChangesAsync();

        var handler = new MarkSubscriptionPaymentFailedHandler(dbContext, TimeProvider.System, eventBusMock.Object);

        await handler.Handle(new MarkSubscriptionPaymentFailedCommand("sub_123", "card declined"), CancellationToken.None);

        var subscription = await dbContext.Subscriptions.Include(x => x.Plan).FirstAsync(x => x.StripeSubscriptionId == "sub_123");
        subscription.Status.Should().Be(SubscriptionStatus.PastDue);

        eventBusMock.Verify(x => x.PublishAsync(
            It.Is<SubscriptionPaymentFailedEvent>(e =>
                e.UserId == user.Id &&
                e.PlanId == plan.Id &&
                e.StripeSubscriptionId == "sub_123" &&
                e.FailureReason == "card declined"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Mark_Artist_Subscription_As_PastDue_And_Publish_Event()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new AppDbContext(options);
        var eventBusMock = new Mock<IEventBus>();
        var utcNow = DateTime.UtcNow;

        var payer = User.Create("payer@mail.com", "hash", "Payer", true, true, utcNow);
        var artist = Artist.Create(payer.Id, "Studio", null, null, null, VerificationStatus.None, utcNow);
        var plan = Plan.Create("Artist Pro", 19.99m, "USD", PlanPeriod.Month, PlanType.Artist, utcNow);
        dbContext.Add(payer);
        dbContext.Add(artist);
        dbContext.Add(plan);
        dbContext.Add(ArtistSubscription.Create(artist.Id, payer.Id, plan.Id, utcNow.AddMonths(-1), utcNow.AddDays(10), SubscriptionStatus.Active, utcNow, "sub_artist_123"));
        await dbContext.SaveChangesAsync();

        var handler = new MarkSubscriptionPaymentFailedHandler(dbContext, TimeProvider.System, eventBusMock.Object);

        await handler.Handle(new MarkSubscriptionPaymentFailedCommand("sub_artist_123", "insufficient funds"), CancellationToken.None);

        var subscription = await dbContext.ArtistSubscriptions.Include(x => x.Plan).FirstAsync(x => x.StripeSubscriptionId == "sub_artist_123");
        subscription.Status.Should().Be(SubscriptionStatus.PastDue);

        eventBusMock.Verify(x => x.PublishAsync(
            It.Is<ArtistSubscriptionPaymentFailedEvent>(e =>
                e.ArtistId == artist.Id &&
                e.PayerUserId == payer.Id &&
                e.PlanId == plan.Id &&
                e.StripeSubscriptionId == "sub_artist_123" &&
                e.FailureReason == "insufficient funds"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
