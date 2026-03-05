using System;
using FluentValidation;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Commands.CreateAlbum;

public sealed class CreateAlbumCommandValidator : AbstractValidator<CreateAlbumCommand>
{
    public CreateAlbumCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);
        
        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description is not null);
        
        RuleFor(x => x.CoverUrl)
            .MaximumLength(2048)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.CoverUrl));
        
        RuleFor(x => x.ReleaseDate)
            .NotEqual(default(DateTime));
        
        RuleFor(x => x.VisibilityStatus)
            .IsInEnum();

        RuleFor(x => x.ArtistIds)
            .NotEmpty()
            .WithMessage("The album must belong to at least one artist.");

        RuleFor(x => x.ScheduledPublishAt)
            .NotEmpty()
            .WithMessage("ScheduledPublishAt is required when visibility is Scheduled.")
            .When(x => x.VisibilityStatus == VisibilityStatus.Scheduled);

        RuleFor(x => x.ScheduledPublishAt)
            .GreaterThan(DateTime.UtcNow) 
            .WithMessage("ScheduledPublishAt must be in the future.")
            .When(x => x.VisibilityStatus == VisibilityStatus.Scheduled && x.ScheduledPublishAt.HasValue);
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}