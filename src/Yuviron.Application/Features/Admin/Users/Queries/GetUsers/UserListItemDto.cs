using System;
using System.Collections.Generic;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.DTOs;

public record UserListItemDto(
    Guid Id,
    string Email,
    string FirstName,
    string? AvatarUrl, 
    AccountState AccountState,
    bool HasActivePremium,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastLoginAt, 
    List<string> Roles
);