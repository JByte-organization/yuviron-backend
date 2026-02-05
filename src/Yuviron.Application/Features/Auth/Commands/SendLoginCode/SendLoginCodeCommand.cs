using MediatR;

namespace Yuviron.Application.Features.Auth.Commands.SendLoginCode;

public record SendLoginCodeCommand(string Email) : IRequest<Unit>;