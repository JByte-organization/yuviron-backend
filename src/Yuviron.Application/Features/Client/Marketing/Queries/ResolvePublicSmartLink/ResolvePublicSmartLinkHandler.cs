using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Marketing.Queries.ResolvePublicSmartLink;

public sealed class ResolvePublicSmartLinkHandler : IRequestHandler<ResolvePublicSmartLinkQuery, ResolvePublicSmartLinkResponse>
{
    private readonly IContentContext _contentContext;
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly IProfileContext _profileContext;
    private readonly TimeProvider _timeProvider;

    public ResolvePublicSmartLinkHandler(
        IContentContext contentContext,
        ICatalogContext catalogContext,
        ILibraryContext libraryContext,
        IProfileContext profileContext,
        TimeProvider timeProvider)
    {
        _contentContext = contentContext;
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _profileContext = profileContext;
        _timeProvider = timeProvider;
    }

    public async Task<ResolvePublicSmartLinkResponse> Handle(ResolvePublicSmartLinkQuery request, CancellationToken cancellationToken)
    {
        var entityId = await ResolveEntityId(request.EntityType, request.PublicId, cancellationToken)
                       ?? throw new NotFoundException(request.EntityType.ToString(), request.PublicId);

        await RecordClickIfSmartCodeExists(request, entityId, cancellationToken);

        var relativePath = GetRelativePath(request.EntityType, entityId);

        return new ResolvePublicSmartLinkResponse(relativePath);
    }

    private async Task<Guid?> ResolveEntityId(SmartLinkType entityType, string publicId, CancellationToken cancellationToken)
    {
        return entityType switch
        {
            SmartLinkType.Album => await _catalogContext.Albums
                .Where(a => a.PublicId == publicId)
                .Select(a => (Guid?)a.Id)
                .FirstOrDefaultAsync(cancellationToken),
            SmartLinkType.Artist => await _catalogContext.Artists
                .Where(a => a.PublicId == publicId)
                .Select(a => (Guid?)a.Id)
                .FirstOrDefaultAsync(cancellationToken),
            SmartLinkType.Track => await _catalogContext.Tracks
                .Where(t => t.PublicId == publicId)
                .Select(t => (Guid?)t.Id)
                .FirstOrDefaultAsync(cancellationToken),
            SmartLinkType.Playlist => await _libraryContext.Playlists
                .Where(p => p.PublicId == publicId)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(cancellationToken),
            SmartLinkType.UserProfile => await _profileContext.UserProfiles
                .Where(p => p.PublicId == publicId)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(cancellationToken),
            _ => null
        };
    }

    private string GetRelativePath(SmartLinkType entityType, Guid entityId)
    {
        return entityType switch
        {
            SmartLinkType.Album => $"/albums/{entityId}",
            SmartLinkType.Artist => $"/artists/{entityId}",
            SmartLinkType.Track => $"/tracks/{entityId}",
            SmartLinkType.Playlist => $"/playlists/{entityId}",
            SmartLinkType.UserProfile => $"/users/{entityId}",
            _ => "/"
        };
    }

    private async Task RecordClickIfSmartCodeExists(
        ResolvePublicSmartLinkQuery request,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var code = request.Code.Trim().ToLowerInvariant();

        var smartLink = await _contentContext.SmartLinks
            .Where(sl =>
                sl.Code == code &&
                sl.EntityType == request.EntityType &&
                sl.EntityId == entityId)
            .FirstOrDefaultAsync(cancellationToken);

        if (smartLink == null || smartLink.IsExpired(utcNow))
        {
            return;
        }

        var click = SmartLinkClick.Create(
            smartLink.Id,
            request.CountryCode,
            request.Referrer,
            request.DeviceType,
            utcNow);

        _contentContext.Add(click);
        await _contentContext.SaveChangesAsync(cancellationToken);
    }
}