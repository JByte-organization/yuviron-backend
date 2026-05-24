using FluentValidation;
using Yuviron.Application.Common;

namespace Yuviron.Application.Common.Utilities;

public abstract class SearchPaginatedValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : PaginatedQuery
{
    protected SearchPaginatedValidator()
    {
        RuleFor(x => x.SearchTerm)
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

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
