namespace Yuviron.Application.Abstractions.Messaging;

public record EmailJob(
    string To,
    string Subject,
    string Body
);