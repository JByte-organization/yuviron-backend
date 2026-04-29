using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Client.Genres.Queries.GetGenres;

public sealed class GetGenresHandler : IRequestHandler<GetGenresQuery, List<GenreItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGenresHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GenreItemDto>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        return await _context.Genres
            .AsNoTracking()
            .Where(g => !g.IsDeleted)
            .OrderBy(g => g.Name) 
            .Take(request.Limit)
            .Select(g => new GenreItemDto(
                g.Id,
                g.Name,
                g.CoverUrl
            ))
            .ToListAsync(cancellationToken);
    }
}