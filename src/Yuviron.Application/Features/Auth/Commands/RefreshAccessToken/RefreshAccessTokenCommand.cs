using MediatR;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Auth.Commands.RefreshAccessToken;

public record RefreshAccessTokenCommand(string RefreshToken) : IRequest<RefreshAccessTokenResponse>, ISensitiveRequest;
