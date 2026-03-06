using FluentValidation;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistValidator : AbstractValidator<UpdatePlaylistCommand>
{
    public UpdatePlaylistValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(150);

        RuleFor(v => v.Description)
            .MaximumLength(2000);

        RuleForEach(v => v.Tracks).ChildRules(track =>
        {
            track.RuleFor(t => t.TrackId).NotEmpty();
            track.RuleFor(t => t.Position).GreaterThanOrEqualTo(0);
        });
    }
}