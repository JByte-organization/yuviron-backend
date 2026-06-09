namespace Yuviron.Application.Features.Client.Appearance.Queries.GetCustomTheme;

public sealed record CustomThemeDto(
    Guid Id,
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor,
    DateTime CreatedAt);
