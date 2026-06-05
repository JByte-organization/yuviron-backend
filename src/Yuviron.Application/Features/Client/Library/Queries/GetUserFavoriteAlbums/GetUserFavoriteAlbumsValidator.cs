using FluentValidation;
using System;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteAlbums;

public sealed class GetUserFavoriteAlbumsValidator : AbstractValidator<GetUserFavoriteAlbumsQuery>
{
    public GetUserFavoriteAlbumsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");
            
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.SortOrder)
            .Must(value => string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase) || 
                           string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortOrder must be 'asc' or 'desc'.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortOrder));
    }
}