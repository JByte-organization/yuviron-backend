using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Artists.Commands.UpdateArtist;

public sealed record UpdateArtistCommand(
    Guid ArtistId,
    string Name,
    string? Bio,
    Guid? AvatarFileId,
    Guid? BannerFileId
) : IRequest, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}