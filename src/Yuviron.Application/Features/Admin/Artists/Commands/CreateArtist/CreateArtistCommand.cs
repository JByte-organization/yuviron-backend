using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;

public sealed record CreateArtistCommand(
    Guid? OwnerUserId,
    string Name,
    string? Bio,
    Guid? AvatarFileId,
    Guid? BannerFileId,
    VerificationStatus VerificationStatus
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}