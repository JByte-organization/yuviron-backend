using FluentValidation;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.ChangeTrackPosition;

public sealed class ChangeTrackPositionValidator : AbstractValidator<ChangeTrackPositionCommand>
{
    public ChangeTrackPositionValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.NewPosition).GreaterThanOrEqualTo(0);
    }
}