using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTracks;

public sealed class GetStudioTracksValidator : AbstractValidator<GetStudioTracksQuery>
{
    public GetStudioTracksValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty().WithMessage("ArtistId is required.");
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

        RuleFor(x => x.SortOrder)
            .Must(x => string.Equals(x, "asc", StringComparison.OrdinalIgnoreCase) || 
                       string.Equals(x, "desc", StringComparison.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.SortOrder));
    }
}