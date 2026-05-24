using FluentValidation;
using System;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserFollowers;

public sealed class GetUserFollowersValidator : AbstractValidator<GetUserFollowersQuery>
{
    public GetUserFollowersValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortOrder)
            .Must(x => string.Equals(x, "asc", StringComparison.OrdinalIgnoreCase) || 
                       string.Equals(x, "desc", StringComparison.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.SortOrder));
    }
}