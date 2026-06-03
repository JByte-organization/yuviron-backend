using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Profile.Queries.GetStudioArtistProfile;

public sealed record GetStudioArtistProfileQuery(Guid ArtistId) 
    : IRequest<StudioArtistProfileDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}