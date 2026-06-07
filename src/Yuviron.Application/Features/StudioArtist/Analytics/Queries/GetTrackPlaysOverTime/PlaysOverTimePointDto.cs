namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackPlaysOverTime;

public record PlaysOverTimePointDto(string Date, int TotalPlays, int UniqueListeners);