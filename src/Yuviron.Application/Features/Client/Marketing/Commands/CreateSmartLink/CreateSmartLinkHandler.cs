using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;
    private readonly string _baseUrl;

    public CreateSmartLinkHandler(
        IContentContext contentContext,
        ICatalogContext catalogContext,
        ILibraryContext libraryContext,
        ICurrentUserService currentUserService,
        TimeProvider timeProvider,
        IOptions<FrontendOptions> frontendOptions)
    {
        _contentContext = contentContext;
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
        _baseUrl = frontendOptions.Value.BaseUrl.TrimEnd('/');
    }

    public async Task<SmartLinkDto> Handle(CreateSmartLinkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId; // Может быть null для анонимов

        bool exists = request.EntityType switch
        {
            SmartLinkType.Track => await _catalogContext.Tracks.AnyAsync(t => t.Id == request.EntityId, cancellationToken),
            SmartLinkType.Album => await _catalogContext.Albums.AnyAsync(a => a.Id == request.EntityId, cancellationToken),
            SmartLinkType.Artist => await _catalogContext.Artists.AnyAsync(a => a.Id == request.EntityId, cancellationToken),
            SmartLinkType.Playlist => await _libraryContext.Playlists.AnyAsync(p => p.Id == request.EntityId, cancellationToken),
            _ => false
        };

        if (!exists)
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
            return new SmartLinkDto(existingLink.Code, $"{_baseUrl}/sl/{existingLink.Code}");
        }

        string code = string.Empty;
        bool codeExists = true;
        int attempts = 0;
        
        while (codeExists && attempts < 10)
        {
            code = Guid.NewGuid().ToString("N")[..8].ToLowerInvariant();
            codeExists = await _contentContext.SmartLinks.AnyAsync(sl => sl.Code == code, cancellationToken);
            attempts++;
        }

        if (codeExists) throw new InvalidOperationException("Could not generate a unique smart link code.");

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

        return new SmartLinkDto(code, $"{_baseUrl}/sl/{code}");
    }
}
