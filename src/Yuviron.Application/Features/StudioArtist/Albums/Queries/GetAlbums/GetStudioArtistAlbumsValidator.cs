using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetAlbums;

public sealed class GetStudioArtistAlbumsValidator : AbstractValidator<GetStudioArtistAlbumsQuery>
{
    public GetStudioArtistAlbumsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize cannot exceed 100.");
    }
}
