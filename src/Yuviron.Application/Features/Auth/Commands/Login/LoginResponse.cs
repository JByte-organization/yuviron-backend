namespace Yuviron.Application.Features.Auth.Commands.Login;

public record LoginResponse(Guid UserId, string Token, string RefreshToken, string Email, 
    HashSet<string> Permissions);