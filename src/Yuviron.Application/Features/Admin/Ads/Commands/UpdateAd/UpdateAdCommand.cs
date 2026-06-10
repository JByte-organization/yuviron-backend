using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Ads.Commands.UpdateAd;

public sealed record UpdateAdCommand(
    Guid AdId, 
    string AdvertiserName, 
    string Title, 
    string? ClickUrl,
    Guid? AudioFileId = null,
    Guid? ImageFileId = null,
    bool IsActive = true
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}