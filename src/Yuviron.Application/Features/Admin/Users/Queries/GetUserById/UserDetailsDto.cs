using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.DTOs;

public record UserDetailsDto(
    Guid Id,
    string Email,
    AccountState AccountState,
    bool AcceptMarketing,
    string? DisplayName,
    DateTime DateOfBirth,
    Gender Gender,
    List<Guid> RoleIds 
);