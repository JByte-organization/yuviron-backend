using System;
using MediatR;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Analytics.Commands.CommitTrackPlay;

public record CommitPlayRequest(
    Guid PlaySessionId, 
    Guid TrackId, 
    PlaybackDeviceType DeviceType, 
    PlaybackSourceType SourceType, 
    Guid? SourceId                
);

public record CommitTrackPlayCommand(
    Guid PlaySessionId, 
    Guid TrackId, 
    Guid UserId,
    PlaybackDeviceType DeviceType,
    PlaybackSourceType SourceType,
    Guid? SourceId,
    string? CountryCode          
) : IRequest<Unit>;