using FluentValidation;

namespace Yuviron.Application.Features.Client.Home.Queries.GetSystemTopArtists;

public sealed class GetSystemTopArtistsValidator : AbstractValidator<GetSystemTopArtistsQuery>
{
    public GetSystemTopArtistsValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("Limit cannot exceed 50.");
    }
}