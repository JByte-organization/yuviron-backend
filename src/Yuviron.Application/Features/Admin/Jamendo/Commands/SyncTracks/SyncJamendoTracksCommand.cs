using MediatR;

namespace Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

public record SyncJamendoTracksCommand(int Limit = 10) : IRequest<int>;