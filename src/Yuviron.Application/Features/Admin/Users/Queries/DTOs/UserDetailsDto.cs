using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.DTOs;

public record UserDetailsDto(
    Guid Id,
    string Email,
    AccountState AccountState,
    bool AcceptMarketing,
    bool AcceptTerms,
    string? DisplayName,
    DateTime DateOfBirth,
    Gender Gender,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<Guid> RoleIds // Список ID ролей для предзаполнения Multi-Select на фронте
);