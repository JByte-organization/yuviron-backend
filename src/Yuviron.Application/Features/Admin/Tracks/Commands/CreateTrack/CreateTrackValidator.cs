using FluentValidation;
using Yuviron.Application.Common;
namespace Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;
public sealed class CreateTrackCommandValidator : AbstractValidator<CreateTrackCommand>
{
    public CreateTrackCommandValidator()
    {
        RuleFor(x => x.AlbumId).
            NotEqual(Guid.Empty)
            .When(x => x.AlbumId.HasValue);
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);
        RuleFor(x => x.DurationMs)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000 * 60 * 60 * 3);
        RuleFor(x => x.AudioStorageKey)
            .NotEmpty()
            .MaximumLength(1024);
        RuleFor(x => x.PreviewStorageKey)
            .MaximumLength(1024)
            .When(x => x.PreviewStorageKey is not null);
        RuleFor(x => x.CoverUrl)
            .MaximumLength(2048)
            .Must(ValidationExtensions.BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.CoverUrl));
        RuleFor(x => x.VisibilityStatus)
            .IsInEnum();
        
        RuleFor(x => x.ArtistIds)
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