using FluentValidation;
using Yuviron.Application.Common.Utilities;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public sealed class GlobalSearchValidator : AbstractValidator<GlobalSearchQuery>
{
    public GlobalSearchValidator()
    {
        RuleFor(x => x.Query)
            .Custom((query, context) =>
            {
                var normalizedQuery = SearchQueryNormalizer.Normalize(query);

                if (normalizedQuery.Length == 0)
                {
                    context.AddFailure("Search query cannot be empty.");
                    return;
                }

                if (normalizedQuery.Length < 2)
                {
                    context.AddFailure("Search query must be at least 2 characters long.");
                }

                if (normalizedQuery.Length > 100)
                {
                    context.AddFailure("Search query is too long.");
                }
            });

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 20).WithMessage("Limit must be between 1 and 20.");
    }
}
