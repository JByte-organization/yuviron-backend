using MediatR;

namespace Yuviron.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Token) : IRequest<Unit>;