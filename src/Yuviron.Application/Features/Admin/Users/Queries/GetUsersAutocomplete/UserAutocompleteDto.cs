
namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsersAutocomplete;

public sealed record UserAutocompleteDto(
    Guid Id,
    string Email,
    string FirstName,
    string? AvatarUrl
);