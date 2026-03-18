using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylists;

public sealed class GetPlaylistsHandler : IRequestHandler<GetPlaylistsQuery, PaginatedList<PlaylistDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPlaylistsHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<PlaylistDto>> Handle(GetPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Playlists.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => p.Title.Contains(request.SearchTerm));
        }

        var projectedQuery = query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PlaylistDto(
                p.Id,
                p.Title,
                p.CoverUrl, 
                p.Visibility,
                p.IsEditorial,
                p.IsEditorial ? "YUVIRON" : (p.User != null ? p.User.Email : "Unknown"),
                p.PlaylistTracks.Count,
                p.CreatedAt,
                p.UpdatedAt
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}