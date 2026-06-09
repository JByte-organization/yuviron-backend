using MediatR;

namespace Yuviron.Application.Features.Auth.Commands.LogoutEverywhere;

public record LogoutEverywhereCommand() : IRequest<Unit>;
