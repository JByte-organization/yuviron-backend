using FluentValidation;

namespace Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;

public sealed class GetUserRecentlyPlayedValidator : AbstractValidator<GetUserRecentlyPlayedQuery>
{
    public GetUserRecentlyPlayedValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(50);
    }
}