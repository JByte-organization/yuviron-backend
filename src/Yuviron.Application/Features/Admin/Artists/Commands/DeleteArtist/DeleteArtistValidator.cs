using FluentValidation;

namespace Yuviron.Application.Features.Admin.Artists.Commands.DeleteArtist;

public sealed class DeleteArtistCommandValidator : AbstractValidator<DeleteArtistCommand>
{
    public DeleteArtistCommandValidator()
    {
        RuleFor(x => x.ArtistId)
            .NotEqual(Guid.Empty);
    }
}