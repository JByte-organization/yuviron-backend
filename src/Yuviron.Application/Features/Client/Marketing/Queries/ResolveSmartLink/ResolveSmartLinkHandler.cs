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
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public ResolveSmartLinkHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<ResolveSmartLinkResponse> Handle(ResolveSmartLinkQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var smartLink = await _context.SmartLinks
                            .FirstOrDefaultAsync(sl => sl.Code == request.Code.ToLowerInvariant(), cancellationToken)
                        ?? throw new NotFoundException("SmartLink", request.Code);

        if (smartLink.IsExpired(utcNow))
        {
            return new ResolveSmartLinkResponse("/"); 
        }

        smartLink.RecordClick(request.CountryCode, request.Referrer, request.DeviceType, utcNow);
        await _context.SaveChangesAsync(cancellationToken);

        string relativePath = smartLink.EntityType switch
        {
            SmartLinkType.Album => $"/albums/{smartLink.EntityId}",
            SmartLinkType.Track => $"/tracks/{smartLink.EntityId}",
            SmartLinkType.Playlist => $"/playlists/{smartLink.EntityId}",
            SmartLinkType.Artist => $"/artists/{smartLink.EntityId}",
            _ => "/" 
        };

        return new ResolveSmartLinkResponse(relativePath);
    }
}