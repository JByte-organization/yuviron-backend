using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTrackById;

public sealed class GetStudioTrackByIdValidator : AbstractValidator<GetStudioTrackByIdQuery>
{
    public GetStudioTrackByIdValidator()
    {
        RuleFor(x => x.TrackId)
            .NotEmpty()
            .WithMessage("Track ID is required.");
    }
}