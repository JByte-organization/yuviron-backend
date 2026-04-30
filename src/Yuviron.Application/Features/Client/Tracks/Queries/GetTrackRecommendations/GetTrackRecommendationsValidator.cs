using FluentValidation;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackRecommendations;

public sealed class GetTrackRecommendationsValidator : AbstractValidator<GetTrackRecommendationsQuery>
{
    public GetTrackRecommendationsValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.Limit).InclusiveBetween(1, 50).WithMessage("Limit must be between 1 and 50.");
    }
}