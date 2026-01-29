using MediatR;

namespace Yuviron.Application.Features.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileVm>;