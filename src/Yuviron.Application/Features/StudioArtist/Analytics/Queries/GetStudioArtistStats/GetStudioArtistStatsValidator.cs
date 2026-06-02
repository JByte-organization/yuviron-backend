using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetStudioArtistStats;

public sealed class GetStudioArtistStatsValidator : AbstractValidator<GetStudioArtistStatsQuery>
{
    public GetStudioArtistStatsValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
    }
}