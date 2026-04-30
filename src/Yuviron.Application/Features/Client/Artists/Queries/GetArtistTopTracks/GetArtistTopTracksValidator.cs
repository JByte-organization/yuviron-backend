using FluentValidation;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistTopTracks;

public sealed class GetArtistTopTracksValidator : AbstractValidator<GetArtistTopTracksQuery>
{
    public GetArtistTopTracksValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.Limit).InclusiveBetween(1, 50).WithMessage("Limit must be between 1 and 50.");
    }
}