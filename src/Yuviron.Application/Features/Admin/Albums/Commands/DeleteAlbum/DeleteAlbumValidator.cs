using System;
using FluentValidation;

namespace Yuviron.Application.Features.Admin.Albums.Commands.DeleteAlbum;

public sealed class DeleteAlbumCommandValidator : AbstractValidator<DeleteAlbumCommand>
{
    public DeleteAlbumCommandValidator()
    {
        RuleFor(x => x.AlbumId)
            .NotEqual(Guid.Empty)
            .WithMessage("AlbumId must be a valid GUID.");
    }
}