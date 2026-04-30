using MediatR;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackRecommendations;

public record GetTrackRecommendationsQuery(Guid TrackId, int Limit = 10) : IRequest<List<RecommendedTrackDto>>;