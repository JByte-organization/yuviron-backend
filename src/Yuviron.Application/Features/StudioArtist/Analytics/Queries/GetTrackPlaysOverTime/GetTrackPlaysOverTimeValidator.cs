using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackPlaysOverTime;

public sealed class GetTrackPlaysOverTimeValidator : AbstractValidator<GetTrackPlaysOverTimeQuery>
{
    public GetTrackPlaysOverTimeValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.Days).InclusiveBetween(7, 365); 
    }
}