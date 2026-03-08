using System;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.DTOs;

public sealed record ArtistTeamMemberDto(
    Guid UserId,
    string Email,
    string? DisplayName, // <-- Добавили имя
    ArtistTeamRole Role
);