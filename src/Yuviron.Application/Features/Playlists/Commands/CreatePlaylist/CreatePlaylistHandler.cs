using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Playlists.Commands.CreatePlaylist
{
    public class CreatePlaylistHandler : IRequestHandler<CreatePlaylistCommand, Guid>
    {
        readonly public IApplicationDbContext _context;
        public CreatePlaylistHandler(IApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<Guid> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
        {
            var playlist = Playlist.Create(
                request.UserId,
                request.Title,
                request.Description,
                request.CoverUrl,
                request.IsPublic
            );

            _context.Playlists.Add(playlist);

            await _context.SaveChangesAsync(cancellationToken);

            return playlist.Id;
        }
    }
}
