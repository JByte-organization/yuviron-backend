using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.UpdateAlbum;

public sealed record UpdateAlbumCommand(
    Guid AlbumId,
    string Title,
    string? Description,
    Guid? CoverFileId,
    ReleaseType ReleaseType,
    DateTime ReleaseDate
) : IRequest, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}