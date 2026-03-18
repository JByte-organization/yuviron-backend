using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.DTOs;

public record RoleSimpleDto(Guid Id, string Name);

public record UserDetailsDto(
    Guid Id,
    string Email,
    AccountState AccountState,
    bool AcceptMarketing,
    bool AcceptTerms,
    string? DisplayName,
    string? AvatarUrl,
    string? Country,
    string? Bio,
    DateTime DateOfBirth,
    Gender Gender,
    bool HasActivePremium,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastLoginAt,
    List<RoleSimpleDto> Roles
);