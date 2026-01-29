using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;
using static Yuviron.Application.Features.Playlists.Queries.GetPlaylistById.GetPlaylistDTO;

namespace Yuviron.Application.Features.Playlists.Queries.GetPlaylistById
{
    public class GetPlaylistHandler : IRequestHandler<GetPlaylistQuery, GetPlaylistDto>
    {
        private readonly IApplicationDbContext _context;

        public GetPlaylistHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetPlaylistDto> Handle(GetPlaylistQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.Playlists
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException("Playlist", request.Id);
            }

            if (!entity.IsPublic)
            {
                if (request.CurrentUserId != entity.UserId)
                {
                    throw new NotFoundException("Playlist", request.Id);
                }
            }

            return new GetPlaylistDto(
                entity.Id,
                entity.Title,
                entity.Description,
                entity.CoverUrl,
                entity.IsPublic,
                entity.UserId
            );
        }
    }
}
