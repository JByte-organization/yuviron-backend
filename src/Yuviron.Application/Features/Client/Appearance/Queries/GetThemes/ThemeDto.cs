namespace Yuviron.Application.Features.Client.Appearance.Queries.GetThemes;

public sealed record ThemeDto(
    Guid Id,
    string Name,
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor,
    bool IsSystem,
    bool IsPremiumOnly,
    bool IsOwnedByCurrentUser,
    bool IsSelected);
