namespace Yuviron.Application.Abstractions.Payment;

public interface IStripeWebhookParser
{
    Task ProcessWebhookAsync(string jsonPayload, string signature, CancellationToken cancellationToken = default);
}