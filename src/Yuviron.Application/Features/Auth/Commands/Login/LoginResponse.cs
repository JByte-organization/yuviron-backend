using System.Text.Json.Serialization;

namespace Yuviron.Application.Features.Auth.Commands.Login;

public record LoginResponse(
    Guid UserId, 
    string Token, 
    [property: JsonIgnore] string RefreshToken,
    string Email, 
    HashSet<string> Permissions);