using FluentValidation;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbums;

public sealed class GetAlbumsValidator : AbstractValidator<GetAlbumsQuery>
{
    public GetAlbumsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term is too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));
            
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid visibility status.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.SortOrder)
            .Must(x => string.Equals(x, "asc", StringComparison.OrdinalIgnoreCase) || 
                       string.Equals(x, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortOrder must be 'asc' or 'desc'.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortOrder));
    }
}