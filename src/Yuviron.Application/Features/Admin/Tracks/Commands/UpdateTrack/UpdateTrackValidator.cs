using System;
using FluentValidation;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrack;

public sealed class UpdateTrackCommandValidator : AbstractValidator<UpdateTrackCommand>
{
    public UpdateTrackCommandValidator()
    {
        RuleFor(x => x.TrackId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.AlbumId)
            .NotEqual(Guid.Empty)
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
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.CoverUrl));

        RuleFor(x => x.VisibilityStatus)
            .IsInEnum();

        // --- ДОБАВЛЕНО: Проверка коллекций ---
        RuleFor(x => x.ArtistIds)
            .NotEmpty()
            .WithMessage("Track must have at least one artist.");
            
        RuleFor(x => x.GenreIds)
            .NotEmpty()
            .WithMessage("Track must have at least one genre.");
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}