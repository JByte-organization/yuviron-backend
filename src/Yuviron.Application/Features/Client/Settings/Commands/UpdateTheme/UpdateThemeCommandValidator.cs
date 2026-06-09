using FluentValidation;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateTheme;

public sealed class UpdateThemeCommandValidator : AbstractValidator<UpdateThemeCommand>
{
    public UpdateThemeCommandValidator()
    {
        RuleFor(x => x.ThemeMode)
            .IsInEnum()
            .WithMessage("Theme mode must be system, dark, or white.");
    }
}
