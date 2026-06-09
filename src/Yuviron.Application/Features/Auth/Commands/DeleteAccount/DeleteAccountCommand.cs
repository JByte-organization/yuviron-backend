using MediatR;

namespace Yuviron.Application.Features.Auth.Commands.DeleteAccount;

public record DeleteAccountCommand() : IRequest<Unit>;
