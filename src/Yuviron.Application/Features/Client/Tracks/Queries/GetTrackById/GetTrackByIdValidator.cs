using FluentValidation;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackById;

public sealed class GetTrackByIdValidator : AbstractValidator<GetTrackByIdQuery>
{
    public GetTrackByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Track ID is required.");
    }
}