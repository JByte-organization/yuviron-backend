using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuviron.Application.Features.Playlists.Commands.CreatePlaylist
{
    public record CreatePlaylistCommand(string Title, string Description, string CoverUrl, bool IsPublic, Guid UserId) : IRequest<Guid>;

}
