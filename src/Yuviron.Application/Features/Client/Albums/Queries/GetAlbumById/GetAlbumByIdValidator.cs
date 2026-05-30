using FluentValidation;

namespace Yuviron.Application.Features.Client.Albums.Queries.GetAlbumById;

public sealed class GetAlbumByIdValidator : AbstractValidator<GetAlbumByIdQuery>
{
    public GetAlbumByIdValidator()
    {
        RuleFor(x => x.AlbumId)
            .NotEmpty().WithMessage("Album ID is required.");
    }
}
