namespace Yuviron.Application.Features.Auth.Commands.Login;

public record LoginResponse(Guid UserId, string Token, string Email);