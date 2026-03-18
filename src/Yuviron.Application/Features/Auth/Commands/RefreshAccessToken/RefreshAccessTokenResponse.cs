using System.Text.Json.Serialization;

namespace Yuviron.Application.Features.Auth.Commands.RefreshAccessToken;

public record RefreshAccessTokenResponse(
    string AccessToken, 
    [property: JsonIgnore] string RefreshToken);