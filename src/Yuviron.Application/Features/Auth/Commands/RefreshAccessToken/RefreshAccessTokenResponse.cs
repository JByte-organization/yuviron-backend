namespace Yuviron.Application.Features.Auth.Commands.RefreshAccessToken;


public record RefreshAccessTokenResponse(string AccessToken, string RefreshToken);