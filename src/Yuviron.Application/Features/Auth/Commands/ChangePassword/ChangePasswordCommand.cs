using MediatR;

using Yuviron.Application.Abstractions;

public sealed record ChangePasswordCommand (
    string OldPassword,
    string NewPassword
) : IRequest<Unit>, ISensitiveRequest;