using System;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackStreamUrl;

public record TrackStreamUrlResponse(string AudioUrl, AdPlaybackDto? PendingAd);

public record AdPlaybackDto(Guid AdId, string AudioUrl, string ImageUrl, string AdvertiserName, string Title, string? ClickUrl);