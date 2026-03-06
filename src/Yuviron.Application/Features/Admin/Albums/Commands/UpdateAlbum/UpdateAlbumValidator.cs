using System;
using FluentValidation;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Commands.UpdateAlbum;

public sealed class UpdateAlbumCommandValidator : AbstractValidator<UpdateAlbumCommand>
{
    // Добавляем TimeProvider в конструктор
    public UpdateAlbumCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.AlbumId)
            .NotEqual(Guid.Empty);
            
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);
            
        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description is not null);
            
        RuleFor(x => x.CoverUrl)
            .MaximumLength(2048)
            .Must(ValidationExtensions.BeValidUrl)
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
            .Must((command, scheduledAt) => scheduledAt > timeProvider.GetUtcNow().UtcDateTime.AddMinutes(-1)) 
            .WithMessage("Scheduled date must be in the future.")
            .When(x => x.VisibilityStatus == VisibilityStatus.Scheduled && x.ScheduledPublishAt.HasValue);
    }
}