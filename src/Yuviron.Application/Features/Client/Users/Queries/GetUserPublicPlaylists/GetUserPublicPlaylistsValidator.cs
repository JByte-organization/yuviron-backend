using FluentValidation;
using System;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserPublicPlaylists;

public sealed class GetUserPublicPlaylistsValidator : AbstractValidator<GetUserPublicPlaylistsQuery>
{
    public GetUserPublicPlaylistsValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target User ID is required.");
            
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");
            
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.SortOrder)
            .Must(x => string.Equals(x, "asc", StringComparison.OrdinalIgnoreCase) || 
                       string.Equals(x, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortOrder must be 'asc' or 'desc'.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortOrder));
    }
}