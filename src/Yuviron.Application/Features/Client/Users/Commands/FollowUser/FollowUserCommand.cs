using MediatR;

namespace Yuviron.Application.Features.Client.Users.Commands.FollowUser;

public sealed record FollowUserCommand(Guid TargetUserId) : IRequest<Unit>;