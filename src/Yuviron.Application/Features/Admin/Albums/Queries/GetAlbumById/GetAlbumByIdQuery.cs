using System;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumById;

public sealed record GetAlbumByIdQuery(Guid AlbumId) : IRequest<AlbumDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}