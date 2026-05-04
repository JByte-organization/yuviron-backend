using FluentValidation;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumsAutocomplete;

public sealed class GetAlbumsAutocompleteValidator : AbstractValidator<GetAlbumsAutocompleteQuery>
{
    public GetAlbumsAutocompleteValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage("Search term cannot be empty.")
            .MinimumLength(2).WithMessage("Please enter at least 2 characters for autocomplete.")
            .MaximumLength(100).WithMessage("Search term is too long.");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Limit cannot exceed 50.");
    }
}