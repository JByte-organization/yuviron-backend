using FluentValidation;

namespace Yuviron.Application.Features.Client.Library.Commands.TrackPlay;

public sealed class TrackPlayValidator : AbstractValidator<TrackPlayCommand>
{
    public TrackPlayValidator()
    {
        RuleFor(x => x.TrackId).NotEqual(Guid.Empty);
    }
}
