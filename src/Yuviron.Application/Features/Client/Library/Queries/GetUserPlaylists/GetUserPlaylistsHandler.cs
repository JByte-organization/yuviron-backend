using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserPlaylists;

public sealed class GetUserPlaylistsHandler : IRequestHandler<GetUserPlaylistsQuery, PaginatedList<UserPlaylistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserPlaylistsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<UserPlaylistDto>> Handle(GetUserPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var projectedQuery = _context.Playlists
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new UserPlaylistDto(
                p.Id,
                p.Title,
                p.CoverUrl,
                p.Visibility,
                p.PlaylistTracks.Count,
                p.CreatedAt,
                p.UpdatedAt
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
