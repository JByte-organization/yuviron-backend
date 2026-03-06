using MediatR;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Unit>, ISensitiveRequest;