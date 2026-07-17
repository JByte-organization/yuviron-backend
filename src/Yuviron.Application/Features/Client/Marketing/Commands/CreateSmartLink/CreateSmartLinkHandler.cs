using Yuviron.Application.Abstractions.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Marketing.Commands.CreateSmartLink;

public sealed class CreateSmartLinkHandler : IRequestHandler<CreateSmartLinkCommand, SmartLinkDto>
{
    private readonly IContentContext _contentContext;
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly IProfileContext _profileContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;
    private readonly string _baseUrl;

    public CreateSmartLinkHandler(
        IContentContext contentContext,
        ICatalogContext catalogContext,
        ILibraryContext libraryContext,
        IProfileContext profileContext,
        ICurrentUserService currentUserService,
        TimeProvider timeProvider,
        IOptions<FrontendOptions> frontendOptions)
    {
        _contentContext = contentContext;
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _profileContext = profileContext;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
        _baseUrl = frontendOptions.Value.BaseUrl.TrimEnd('/');
    }

    public async Task<SmartLinkDto> Handle(CreateSmartLinkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        string? publicId = await GetPublicId(request.EntityType, request.EntityId, cancellationToken);

        if (string.IsNullOrWhiteSpace(publicId))
        {
            throw new NotFoundException(request.EntityType.ToString(), request.EntityId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var existingLink = await _contentContext.SmartLinks
            .FirstOrDefaultAsync(sl => 
                sl.EntityType == request.EntityType && 
                sl.EntityId == request.EntityId && 
                sl.CreatedByUserId == userId &&
                (sl.ExpiresAt == null || sl.ExpiresAt > utcNow), 
                cancellationToken);

        if (existingLink != null)
        {
            return new SmartLinkDto(existingLink.Code, BuildPublicUrl(request.EntityType, publicId, existingLink.Code));
        }

        string code = await GenerateUniqueCode(cancellationToken);

        var smartLink = SmartLink.Create(
            code: code,
            entityType: request.EntityType,
            entityId: request.EntityId,
            createdByUserId: userId,
            expiresAt: null, 
            utcNow: utcNow
        );

        _contentContext.Add(smartLink);
        await _contentContext.SaveChangesAsync(cancellationToken);

        return new SmartLinkDto(code, BuildPublicUrl(request.EntityType, publicId, code));
    }

    private async Task<string?> GetPublicId(SmartLinkType entityType, Guid entityId, CancellationToken cancellationToken)
    {
        return entityType switch
        {
            SmartLinkType.Track => await _catalogContext.Tracks
                .Where(t => t.Id == entityId)
                .Select(t => t.PublicId)
                .FirstOrDefaultAsync(cancellationToken),
            SmartLinkType.Album => await _catalogContext.Albums
                .Where(a => a.Id == entityId)
                .Select(a => a.PublicId)
                .FirstOrDefaultAsync(cancellationToken),
            SmartLinkType.Artist => await _catalogContext.Artists
                .Where(a => a.Id == entityId)
                .Select(a => a.PublicId)
                .FirstOrDefaultAsync(cancellationToken),
            SmartLinkType.Playlist => await _libraryContext.Playlists
                .Where(p => p.Id == entityId)
                .Select(p => p.PublicId)
                .FirstOrDefaultAsync(cancellationToken),
            SmartLinkType.UserProfile => await _profileContext.UserProfiles
                .Where(p => p.Id == entityId)
                .Select(p => p.PublicId)
                .FirstOrDefaultAsync(cancellationToken),
            _ => null
        };
    }

    private async Task<string> GenerateUniqueCode(CancellationToken cancellationToken)
    {
        string code;
        bool codeExists;
        int attempts = 0;
        
        do
        {
            code = Guid.NewGuid().ToString("N")[..8].ToLowerInvariant();
            codeExists = await _contentContext.SmartLinks.AnyAsync(sl => sl.Code == code, cancellationToken);
            attempts++;
        } while (codeExists && attempts < 10);

        if (codeExists)
        {
            throw new InvalidOperationException("Could not generate a unique smart link code.");
        }

        return code;
    }

    private string BuildPublicUrl(SmartLinkType entityType, string publicId, string code)
    {
        var segment = entityType switch
        {
            SmartLinkType.Album => "album",
            SmartLinkType.Artist => "artist",
            SmartLinkType.Track => "track",
            SmartLinkType.Playlist => "playlist",
            SmartLinkType.UserProfile => "user",
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, "Unsupported smart link entity type.")
        };

        return $"{_baseUrl}/{segment}/{publicId}?si={code}";
    }
}