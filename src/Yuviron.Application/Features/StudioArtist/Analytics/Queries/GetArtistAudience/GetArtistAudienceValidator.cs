using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistAudience;

public sealed class GetArtistAudienceValidator : AbstractValidator<GetArtistAudienceQuery>
{
    public GetArtistAudienceValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.Days).InclusiveBetween(7, 365);
    }
}