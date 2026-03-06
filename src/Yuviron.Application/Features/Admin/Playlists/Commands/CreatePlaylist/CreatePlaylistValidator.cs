using FluentValidation;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistValidator : AbstractValidator<CreatePlaylistCommand>
{
    public CreatePlaylistValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(150).WithMessage("Title must not exceed 150 characters");

        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("Description is too long");

        RuleForEach(v => v.Tracks).ChildRules(track =>
        {
            track.RuleFor(t => t.TrackId).NotEmpty().WithMessage("Track ID is required");
            track.RuleFor(t => t.Position).GreaterThanOrEqualTo(0).WithMessage("Position must be non-negative");
        });
    }
}