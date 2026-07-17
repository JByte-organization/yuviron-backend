using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Configuration;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Marketing.Queries.ResolveSmartLink;

public sealed class ResolveSmartLinkHandler : IRequestHandler<ResolveSmartLinkQuery, ResolveSmartLinkResponse>
{
    private readonly IContentContext _contentContext;
    private readonly TimeProvider _timeProvider;

    public ResolveSmartLinkHandler(IContentContext contentContext, TimeProvider timeProvider)
    {
        _contentContext = contentContext;
        _timeProvider = timeProvider;
    }

    public async Task<ResolveSmartLinkResponse> Handle(ResolveSmartLinkQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var smartLink = await _contentContext.SmartLinks
                            .FirstOrDefaultAsync(sl => sl.Code == request.Code.ToLowerInvariant(), cancellationToken)
                        ?? throw new NotFoundException("SmartLink", request.Code);

        if (smartLink.IsExpired(utcNow))
        {
            return new ResolveSmartLinkResponse("/"); 
        }

        var click = SmartLinkClick.Create(
            smartLink.Id,
            request.CountryCode,
            request.Referrer,
            request.DeviceType,
            utcNow);

        _contentContext.Add(click);
        await _contentContext.SaveChangesAsync(cancellationToken);

        string relativePath = smartLink.EntityType switch
        {
            SmartLinkType.Album => $"/albums/{smartLink.EntityId}",
            SmartLinkType.Track => $"/tracks/{smartLink.EntityId}",
            SmartLinkType.Playlist => $"/playlists/{smartLink.EntityId}",
            SmartLinkType.Artist => $"/artists/{smartLink.EntityId}",
            SmartLinkType.UserProfile => $"/users/{smartLink.EntityId}",
            _ => "/" 
        };

        return new ResolveSmartLinkResponse(relativePath);
    }
}
