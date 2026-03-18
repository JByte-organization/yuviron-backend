using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.DTOs;

public record UserListItemDto(
    Guid Id,
    string Email,
    string? DisplayName,
    string? AvatarUrl, 
    AccountState AccountState,
    bool HasActivePremium,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastLoginAt, 
    List<string> Roles
);