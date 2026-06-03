using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Team.Queries.GetTeamMembers;

public record TeamMemberDto(
    Guid UserId,
    string Email,
    string FirstName,
    string? AvatarUrl,
    ArtistTeamRole Role,
    DateTime AddedAt
);