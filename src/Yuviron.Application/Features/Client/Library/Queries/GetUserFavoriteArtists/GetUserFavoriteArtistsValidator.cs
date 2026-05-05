using FluentValidation;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteArtists;

public sealed class GetUserFavoriteArtistsValidator : AbstractValidator<GetUserFavoriteArtistsQuery>
{
    public GetUserFavoriteArtistsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
