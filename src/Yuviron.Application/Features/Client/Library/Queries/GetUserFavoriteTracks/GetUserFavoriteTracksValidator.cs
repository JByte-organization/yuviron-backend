using FluentValidation;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteTracks;

public sealed class GetUserFavoriteTracksValidator : AbstractValidator<GetUserFavoriteTracksQuery>
{
    public GetUserFavoriteTracksValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy)
            .Must(value => value is null
                || value.Equals("savedAt", StringComparison.OrdinalIgnoreCase)
                || value.Equals("title", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortBy must be either 'savedAt' or 'title'.");
        RuleFor(x => x.SortOrder)
            .Must(value => value is null
                || value.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || value.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortOrder must be either 'asc' or 'desc'.");
    }
}
