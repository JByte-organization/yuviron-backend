using FluentValidation;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public sealed class GlobalSearchValidator : AbstractValidator<GlobalSearchQuery>
{
    public GlobalSearchValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Search query cannot be empty.")
            .MinimumLength(2).WithMessage("Search query must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Search query is too long.");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 20).WithMessage("Limit must be between 1 and 20.");
    }
}