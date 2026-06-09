using FluentValidation;

namespace Yuviron.Application.Features.Client.Appearance.Commands.UpdateThemePreset;

public sealed class UpdateThemePresetCommandValidator : AbstractValidator<UpdateThemePresetCommand>
{
    public UpdateThemePresetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
        RuleFor(x => x.PrimaryColor).NotEmpty().MaximumLength(32);
        RuleFor(x => x.SecondaryColor).NotEmpty().MaximumLength(32);
        RuleFor(x => x.BackgroundColor).NotEmpty().MaximumLength(32);
    }
}
