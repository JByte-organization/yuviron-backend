using FluentValidation;

namespace Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks;

public sealed class GetUserTopTracksValidator : AbstractValidator<GetUserTopTracksQuery>
{
    public GetUserTopTracksValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Limit cannot exceed 50.");
    }
}