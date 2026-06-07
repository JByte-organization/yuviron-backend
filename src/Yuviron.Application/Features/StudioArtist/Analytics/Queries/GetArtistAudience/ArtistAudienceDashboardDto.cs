using System.Collections.Generic;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistAudience;

public record AudienceGeographyDto(string CountryCode, int Listeners);
public record AudienceDeviceDto(string DeviceType, int Plays);

public record ArtistAudienceDashboardDto(
    List<AudienceGeographyDto> TopCountries,
    List<AudienceDeviceDto> Devices
);