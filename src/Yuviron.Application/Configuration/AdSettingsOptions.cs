namespace Yuviron.Application.Configuration;

public class AdSettingsOptions
{
    public const string SectionName = "AdSettings";

    public int CooldownMinutes { get; set; } = 15; 
}