using MediatR;

namespace Yuviron.Application.Features.Auth.Commands.RefreshAccessToken;

public record RefreshAccessTokenCommand(string RefreshToken) : IRequest<RefreshAccessTokenResponse>;

public record RefreshAccessTokenResponse(string AccessToken, string RefreshToken);