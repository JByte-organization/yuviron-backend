using System;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistById;

public sealed record GetArtistByIdQuery(Guid ArtistId) : IRequest<ArtistDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}