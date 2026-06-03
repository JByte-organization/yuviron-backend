using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Finance.Commands.RequestPayout;

public sealed class RequestPayoutValidator : AbstractValidator<RequestPayoutCommand>
{
    public RequestPayoutValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payout amount must be greater than zero.");
    }
}