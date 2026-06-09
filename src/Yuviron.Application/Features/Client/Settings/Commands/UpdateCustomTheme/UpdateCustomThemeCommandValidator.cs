using FluentValidation;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateCustomTheme;

public sealed class UpdateCustomThemeCommandValidator : AbstractValidator<UpdateCustomThemeCommand>
{
    public UpdateCustomThemeCommandValidator()
    {
        RuleFor(x => x.PrimaryColor)
            .NotEmpty().WithMessage("Primary color is required.")
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Primary color must be a hex color like #112233.");

        RuleFor(x => x.SecondaryColor)
            .NotEmpty().WithMessage("Secondary color is required.")
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Secondary color must be a hex color like #112233.");

        RuleFor(x => x.BackgroundColor)
            .NotEmpty().WithMessage("Background color is required.")
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Background color must be a hex color like #112233.");
    }
}
