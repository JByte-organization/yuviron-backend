using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylistById;

public sealed record GetStudioPlaylistByIdQuery(Guid PlaylistId) 
    : IRequest<StudioPlaylistDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}