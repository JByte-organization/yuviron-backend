using FluentValidation;

namespace Yuviron.Application.Features.Admin.Themes.Commands.UpdateTheme;

public sealed class UpdateThemeValidator : AbstractValidator<UpdateThemeCommand>
{
    public UpdateThemeValidator()
    {
        RuleFor(x => x.ThemeId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.PrimaryColor)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.SecondaryColor)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.BackgroundColor)
            .NotEmpty()
            .MaximumLength(32);
    }
}
