using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.CreateAlbum;

public sealed record CreateAlbumCommand(
    Guid ArtistId,
    string Title,
    string? Description,
    Guid? CoverFileId, 
    ReleaseType ReleaseType,
    DateTime? ReleaseDate
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}