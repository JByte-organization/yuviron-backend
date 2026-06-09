using MediatR;
using System;

namespace Yuviron.Application.Features.Client.Playlists.Commands.RecoverPlaylist;

public record RecoverPlaylistCommand(Guid PlaylistId) : IRequest<Unit>;
