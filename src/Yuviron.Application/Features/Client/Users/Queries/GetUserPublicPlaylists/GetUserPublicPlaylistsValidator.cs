using FluentValidation;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserPublicPlaylists;

public sealed class GetUserPublicPlaylistsValidator : AbstractValidator<GetUserPublicPlaylistsQuery>
{
    public GetUserPublicPlaylistsValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty().WithMessage("Target User ID is required.");
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}