using FluentValidation;

namespace Yuviron.Application.Features.Client.Home.Queries.GetNewReleases;

public sealed class GetNewReleasesValidator : AbstractValidator<GetNewReleasesQuery>
{
    public GetNewReleasesValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Limit cannot exceed 50.");
    }
}