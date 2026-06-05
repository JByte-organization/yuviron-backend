using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.RequestVerification;

public sealed class RequestArtistVerificationValidator : AbstractValidator<RequestArtistVerificationCommand>
{
    public RequestArtistVerificationValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.OfficialEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Links).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Message).MaximumLength(500);
    }
}