using System;
using System.Collections.Generic;
using MediatR;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Analytics.Commands.CommitTrackPlay;

public record PlaybackChunk(int StartSecond, int EndSecond);

public record CommitPlayRequest(
    Guid PlaySessionId, 
    Guid TrackId, 
    PlaybackDeviceType DeviceType, 
    PlaybackSourceType SourceType, 
    Guid? SourceId,
    List<PlaybackChunk>? Chunks 
);

public record CommitTrackPlayCommand(
    Guid PlaySessionId, 
    Guid TrackId, 
    Guid UserId,
    PlaybackDeviceType DeviceType,
    PlaybackSourceType SourceType,
    Guid? SourceId,
    string? CountryCode,
    List<PlaybackChunk>? Chunks 
) : IRequest<Unit>;