using FluentValidation;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserProfile;

public sealed class GetUserProfileValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    }
}