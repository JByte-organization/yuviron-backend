using FluentValidation;

namespace Yuviron.Application.Features.Auth.Commands.RefreshAccessToken;

public sealed class RefreshAccessTokenValidator : AbstractValidator<RefreshAccessTokenCommand>
{
    public RefreshAccessTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(512);
    }
}
