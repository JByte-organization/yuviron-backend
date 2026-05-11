using FluentValidation;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistById;

public sealed class GetPlaylistByIdValidator : AbstractValidator<GetPlaylistByIdQuery>
{
    public GetPlaylistByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Playlist ID is required.");
    }
}