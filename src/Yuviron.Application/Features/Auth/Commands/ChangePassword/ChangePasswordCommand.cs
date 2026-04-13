using MediatR;

using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand (
    string OldPassword,
    string NewPassword
) : IRequest<Unit>, ISensitiveRequest;