using FluentValidation;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateCrossfade;

public sealed class UpdateCrossfadeCommandValidator : AbstractValidator<UpdateCrossfadeCommand>
{
    public UpdateCrossfadeCommandValidator()
    {
        RuleFor(x => x.CrossfadeMs)
            .GreaterThanOrEqualTo(0).WithMessage("Crossfade cannot be negative.");
    }
}
