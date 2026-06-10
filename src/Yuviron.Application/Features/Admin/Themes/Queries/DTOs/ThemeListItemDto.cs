using System;

namespace Yuviron.Application.Features.Admin.Themes.Queries.DTOs;

public record ThemeListItemDto(
    Guid Id,
    string Name,
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor,
    bool IsSystem,
    bool IsPremiumOnly,
    Guid? UserId
);
