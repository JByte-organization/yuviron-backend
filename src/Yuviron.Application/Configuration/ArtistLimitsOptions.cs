namespace Yuviron.Application.Configuration;

public class ArtistLimitsOptions
{
    public const string SectionName = "ArtistLimits";

    public int FreeUserMaxProfiles { get; set; } = 1; 
    public int PremiumUserMaxProfiles { get; set; } = 5;
}