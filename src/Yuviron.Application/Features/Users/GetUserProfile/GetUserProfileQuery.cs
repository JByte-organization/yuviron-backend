using MediatR;

namespace Yuviron.Application.Features.Users.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDTO>;