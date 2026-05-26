using MediatR;
using System;

namespace Yuviron.Application.Features.Admin.Ads.Commands.CreateAd;

public sealed record CreateAdRequest(
    string AdvertiserName,
    string Title, 
    Guid AudioFileId, 
    Guid ImageFileId,
    string? ClickUrl,
    bool IsActive
);

public sealed record CreateAdCommand(
    string AdvertiserName,
    string Title, 
    Guid AudioFileId, 
    Guid ImageFileId,
    string? ClickUrl,
    bool IsActive
) : IRequest<Guid>;