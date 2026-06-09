using MediatR;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Client.Home.Queries.GetPersonalizedRecommendations;

public record GetPersonalizedRecommendationsQuery(int Limit = 20) : IRequest<List<RecommendationTrackDto>>;
