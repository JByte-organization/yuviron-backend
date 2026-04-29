using FluentValidation;

namespace Yuviron.Application.Features.Client.Home.Queries.GetHomeBanners;

public sealed class GetHomeBannersValidator : AbstractValidator<GetHomeBannersQuery>
{
    public GetHomeBannersValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Limit cannot exceed 50.");
    }
}