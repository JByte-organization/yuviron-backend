using FluentValidation;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistById;

public sealed class GetArtistByIdValidator : AbstractValidator<GetArtistByIdQuery>
{
    public GetArtistByIdValidator()
    {
        RuleFor(x => x.ArtistId)
            .NotEmpty()
            .WithMessage("Artist ID is required.");
    }
}
