using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistPlaysOverTime;

public sealed class GetArtistPlaysOverTimeValidator : AbstractValidator<GetArtistPlaysOverTimeQuery>
{
    public GetArtistPlaysOverTimeValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.Days).InclusiveBetween(7, 365);
    }
}