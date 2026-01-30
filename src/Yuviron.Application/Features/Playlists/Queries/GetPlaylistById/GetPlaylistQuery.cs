using MediatR;
using static Yuviron.Application.Features.Playlists.Queries.GetPlaylistById.GetPlaylistDTO;

public record GetPlaylistQuery(
    Guid Id, Guid? CurrentUserId) : IRequest<GetPlaylistDto>;