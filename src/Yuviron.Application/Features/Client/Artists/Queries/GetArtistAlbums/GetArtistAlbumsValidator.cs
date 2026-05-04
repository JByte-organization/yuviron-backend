using FluentValidation;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

public sealed class GetArtistAlbumsValidator : AbstractValidator<GetArtistAlbumsQuery>
{
    public GetArtistAlbumsValidator()
    {
        RuleFor(x => x.ArtistId)
            .NotEmpty().WithMessage("Artist ID is required.");
        
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");
            
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("PageSize must be between 1 and 50.");
    }
}