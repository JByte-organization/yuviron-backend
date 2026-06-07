using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackRetention;

public sealed class GetTrackRetentionValidator : AbstractValidator<GetTrackRetentionQuery>
{
    public GetTrackRetentionValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.TrackId).NotEmpty();
    }
}