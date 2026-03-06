using MediatR;

namespace Yuviron.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Unit>;