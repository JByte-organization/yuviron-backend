using FluentValidation;

namespace Yuviron.Application.Features.Client.Appearance.Commands.CreateThemePreset;

public sealed class CreateThemePresetCommandValidator : AbstractValidator<CreateThemePresetCommand>
{
    public CreateThemePresetCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
        RuleFor(x => x.PrimaryColor).NotEmpty().MaximumLength(32);
        RuleFor(x => x.SecondaryColor).NotEmpty().MaximumLength(32);
        RuleFor(x => x.BackgroundColor).NotEmpty().MaximumLength(32);
    }
}
