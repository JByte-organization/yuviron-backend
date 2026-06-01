namespace Yuviron.Application.Configuration;

public class CorsSettingsOptions
{
    public const string SectionName = "CorsSettings";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}