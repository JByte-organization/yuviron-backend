using FluentValidation;

namespace Yuviron.Application.Features.Client.Genres.Queries.GetGenres;

public sealed class GetGenresValidator : AbstractValidator<GetGenresQuery>
{
    public GetGenresValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Limit cannot exceed 50.");
    }
}