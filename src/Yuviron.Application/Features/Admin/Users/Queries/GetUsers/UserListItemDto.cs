using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.DTOs;

public record UserListItemDto(
    Guid Id,
    string Email,
    string? DisplayName, // Берем из профиля
    AccountState AccountState,
    bool IsDeleted,
    DateTime CreatedAt,
    List<string> Roles
);