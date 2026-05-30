using FluentValidation;

namespace Yuviron.Application.Features.Client.Albums.Queries.GetAlbumTracks;

public sealed class GetAlbumTracksValidator : AbstractValidator<GetAlbumTracksQuery>
{
    public GetAlbumTracksValidator()
    {
        RuleFor(x => x.AlbumId)
            .NotEmpty().WithMessage("Album ID is required.");
    }
}
