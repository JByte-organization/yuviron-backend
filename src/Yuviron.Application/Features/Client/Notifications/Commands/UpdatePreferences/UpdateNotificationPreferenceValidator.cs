using FluentValidation;
using Yuviron.Application.Features.Client.Notifications.Preferences;

namespace Yuviron.Application.Features.Client.Notifications.Commands.UpdatePreferences;

public sealed class UpdateNotificationPreferenceValidator : AbstractValidator<UpdateNotificationPreferenceCommand>
{
    public UpdateNotificationPreferenceValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Category)
            .IsInEnum();
    }
}
