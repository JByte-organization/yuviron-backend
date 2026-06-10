using FluentValidation;

namespace Yuviron.Application.Features.Admin.Themes.Commands.CreateTheme;

public sealed class CreateThemeValidator : AbstractValidator<CreateThemeCommand>
{
    public CreateThemeValidator()
    {
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
