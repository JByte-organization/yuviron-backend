using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Team.Commands.AddTeamMember;

public sealed class AddTeamMemberValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberValidator()
    {
        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Role).IsInEnum();
    }
}