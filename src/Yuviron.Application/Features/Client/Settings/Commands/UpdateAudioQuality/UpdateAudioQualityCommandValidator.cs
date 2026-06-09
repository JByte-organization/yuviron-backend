using FluentValidation;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateAudioQuality;

public sealed class UpdateAudioQualityCommandValidator : AbstractValidator<UpdateAudioQualityCommand>
{
    public UpdateAudioQualityCommandValidator()
    {
        RuleFor(x => x.AudioQualityPreference)
            .GreaterThan(0).WithMessage("Audio quality must be greater than zero.");
    }
}
