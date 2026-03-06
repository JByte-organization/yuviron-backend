using FluentValidation;

namespace Yuviron.Application.Features.Admin.Artists.Commands.RemoveTeamMember;

public sealed class RemoveTeamMemberValidator : AbstractValidator<RemoveTeamMemberCommand>
{
    public RemoveTeamMemberValidator()
    {
        RuleFor(x => x.ArtistId).NotEqual(Guid.Empty);
        RuleFor(x => x.UserId).NotEqual(Guid.Empty);
    }
}