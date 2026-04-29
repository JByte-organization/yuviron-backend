using System;
using MediatR;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Analytics.Commands.CommitTrackPlay;

// То, что присылает фронтенд (JSON)
public record CommitPlayRequest(
    Guid PlaySessionId, 
    Guid TrackId, 
    Guid ArtistId,
    PlaybackDeviceType DeviceType, 
    PlaybackSourceType SourceType, 
    Guid? SourceId                
);

// То, что летит в Handler
public record CommitTrackPlayCommand(
    Guid PlaySessionId, 
    Guid TrackId, 
    Guid ArtistId, 
    Guid UserId,
    PlaybackDeviceType DeviceType,
    PlaybackSourceType SourceType,
    Guid? SourceId,
    string? CountryCode          
) : IRequest<Unit>;