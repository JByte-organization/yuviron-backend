using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;

public sealed class CreateTrackValidator : AbstractValidator<CreateTrackCommand>
{
    public CreateTrackValidator()
    {
        RuleFor(x => x.AlbumId).NotEmpty();
        RuleFor(x => x.AudioFileId).NotEmpty();
        
        RuleFor(x => x.Title).NotEmpty().MaximumLength(256);
            
        RuleForEach(x => x.Collaborators).ChildRules(collaborator => 
        {
            collaborator.RuleFor(c => c.ArtistId).NotEmpty();
            collaborator.RuleFor(c => c.Role).IsInEnum();
        });
    }
}