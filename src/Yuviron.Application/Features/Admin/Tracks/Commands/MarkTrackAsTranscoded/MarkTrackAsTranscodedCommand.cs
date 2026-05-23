using MediatR;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.MarkTrackAsTranscoded;

public record MarkTrackAsTranscodedCommand(
    Guid TrackId,
    string TempAudioStorageKey,
    string HlsUrl,
    string FinalAudioKey
) : IRequest<Unit>;