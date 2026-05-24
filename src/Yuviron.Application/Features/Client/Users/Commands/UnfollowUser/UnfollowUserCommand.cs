using MediatR;

namespace Yuviron.Application.Features.Client.Users.Commands.UnfollowUser;

public sealed record UnfollowUserCommand(Guid TargetUserId) : IRequest<Unit>;