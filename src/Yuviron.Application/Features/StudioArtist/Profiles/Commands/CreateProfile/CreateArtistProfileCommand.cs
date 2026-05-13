using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.CreateProfile;

public sealed record CreateArtistProfileRequest(
    string Name,
    string? AvatarUrl
);

public sealed record CreateArtistProfileResponse(Guid ArtistId);

public sealed record CreateArtistProfileCommand(
    string Name,
    string? AvatarUrl
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistProfile;
}