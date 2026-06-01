using FluentValidation;

namespace Yuviron.Application.Features.Client.Home.Queries.GetSystemTopTracks;

public sealed class GetSystemTopTracksValidator : AbstractValidator<GetSystemTopTracksQuery>
{
    public GetSystemTopTracksValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Limit cannot exceed 50.");
    }
}