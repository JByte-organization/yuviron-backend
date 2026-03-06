using System;
using FluentValidation;

namespace Yuviron.Application.Features.Admin.Artists.Commands.AddTeamMember;

public sealed class AddTeamMemberValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberValidator()
    {
        RuleFor(x => x.ArtistId).NotEqual(Guid.Empty);
        
        RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        
        RuleFor(x => x.Role).IsInEnum(); 
    }
}