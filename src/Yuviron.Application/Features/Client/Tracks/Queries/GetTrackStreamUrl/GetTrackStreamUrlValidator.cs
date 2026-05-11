using FluentValidation;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackStreamUrl;

public sealed class GetTrackStreamUrlValidator : AbstractValidator<GetTrackStreamUrlQuery>
{
    public GetTrackStreamUrlValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
    }
}