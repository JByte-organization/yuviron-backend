using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Ads.Commands.CreateAd;


public sealed record CreateAdCommand(
    string AdvertiserName,
    string Title, 
    Guid AudioFileId, 
    Guid ImageFileId,
    string? ClickUrl,
    bool IsActive
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}