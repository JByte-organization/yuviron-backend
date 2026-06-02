using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;

public sealed class CreateStudioArtistTrackValidator : AbstractValidator<CreateStudioArtistTrackCommand>
{
    public CreateStudioArtistTrackValidator()
    {
        RuleFor(x => x.AlbumId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.AudioFileId)
            .NotEqual(Guid.Empty);

        RuleForEach(x => x.CoAuthorIds!)
            .NotEqual(Guid.Empty)
            .When(x => x.CoAuthorIds is { Count: > 0 });
    }
}
