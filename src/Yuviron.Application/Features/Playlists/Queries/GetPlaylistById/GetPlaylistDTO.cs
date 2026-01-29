using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuviron.Application.Features.Auth.Commands.Register;

namespace Yuviron.Application.Features.Playlists.Queries.GetPlaylistById
{
    public class GetPlaylistDTO : AbstractValidator<GetPlaylistQuery>
    {
        public record GetPlaylistDto(
         Guid Id,
         string Title,
         string? Description,
         string? CoverUrl,
         bool IsPublic,
         Guid UserId
     );
    }
}
