using FluentValidation;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;

public sealed class CreateTrackCommandValidator : AbstractValidator<CreateTrackCommand>
{
    public CreateTrackCommandValidator()
    {
        RuleFor(x => x.AlbumId)
            .NotEqual(Guid.Empty);
        RuleFor(x => x.AlbumPosition)
            .GreaterThan(0);
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);
            
        RuleFor(x => x.AudioStorageKey)
            .NotEmpty()
            .MaximumLength(1024)
            .Must(key => key == null || !key.Contains(".."))
            .WithMessage("Invalid file path.");
            
        RuleFor(x => x.CoverUrl)
            .MaximumLength(2048)
            .Must(url => url == null || !url.Contains(".."))
            .WithMessage("Invalid file path.")
            .When(x => !string.IsNullOrWhiteSpace(x.CoverUrl));
            
        RuleFor(x => x.VisibilityStatus)
            .IsInEnum();

        RuleFor(x => x.Artists)
            .NotEmpty()
            .WithMessage("Track must have at least one artist.");

        RuleFor(x => x.GenreIds)
            .NotEmpty()
            .WithMessage("Track must have at least one genre.");
        RuleFor(x => x.MoodIds)
            .NotEmpty()
            .WithMessage("Track must have at least one mood.");
    }
}