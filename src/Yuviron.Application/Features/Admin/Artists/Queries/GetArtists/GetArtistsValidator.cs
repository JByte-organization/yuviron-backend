using FluentValidation;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtists;

public sealed class GetArtistsValidator : AbstractValidator<GetArtistsQuery>
{
    public GetArtistsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term is too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

        RuleFor(x => x.VerificationStatus)
            .IsInEnum().WithMessage("Invalid verification status.")
            .When(x => x.VerificationStatus.HasValue);

        RuleFor(x => x.SortOrder)
            .Must(x => string.Equals(x, "asc", StringComparison.OrdinalIgnoreCase) || 
                       string.Equals(x, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortOrder must be 'asc' or 'desc'.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortOrder));
    }
}