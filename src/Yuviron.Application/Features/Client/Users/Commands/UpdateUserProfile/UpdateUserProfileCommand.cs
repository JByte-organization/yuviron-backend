using MediatR;

namespace Yuviron.Application.Features.Client.Users.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileRequest(
    string Name,
    string? Bio,
    Guid? AvatarFileId,
    Guid? BannerFileId
);

public sealed record UpdateUserProfileCommand(
    string Name,
    string? Bio,
    Guid? AvatarFileId,
    Guid? BannerFileId
) : IRequest;