using FluentValidation;

namespace Yuviron.Application.Features.Admin.Artists.Commands.UpdateTeamMemberRole;

public sealed class UpdateTeamMemberRoleValidator : AbstractValidator<UpdateTeamMemberRoleCommand>
{
    public UpdateTeamMemberRoleValidator()
    {
        RuleFor(x => x.ArtistId).NotEqual(Guid.Empty);
        
        RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        
        RuleFor(x => x.NewRole).IsInEnum();
    }
}