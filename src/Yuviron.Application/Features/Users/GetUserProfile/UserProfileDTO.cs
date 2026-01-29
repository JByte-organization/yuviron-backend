namespace Yuviron.Application.Features.Users.GetUserProfile;

public record UserProfileDTO(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    DateTime JoinedAt
);