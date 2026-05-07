using FluentValidation;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistPopularReleases;

public sealed class GetArtistPopularReleasesValidator : AbstractValidator<GetArtistPopularReleasesQuery>
{
    public GetArtistPopularReleasesValidator()
    {
        RuleFor(x => x.ArtistId)
            .NotEmpty()
            .WithMessage("Artist ID is required.");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 50)
            .WithMessage("Limit must be between 1 and 50.");
    }
}
