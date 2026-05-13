using MediatR;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Unit>, ISensitiveRequest;