using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

public record SyncJamendoTracksCommand(
    int Limit = 10, 
    int Offset = 0 
) : IRequest<int>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}