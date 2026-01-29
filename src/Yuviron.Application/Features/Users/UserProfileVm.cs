namespace Yuviron.Application.Features.Users.Queries.GetUserProfile;

public record UserProfileVm(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    DateTime JoinedAt
);