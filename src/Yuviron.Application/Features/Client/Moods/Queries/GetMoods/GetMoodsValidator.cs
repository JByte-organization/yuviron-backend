using FluentValidation;

namespace Yuviron.Application.Features.Client.Moods.Queries.GetMoods;

public sealed class GetMoodsValidator : AbstractValidator<GetMoodsQuery>
{
    public GetMoodsValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Limit cannot exceed 50.");
    }
}