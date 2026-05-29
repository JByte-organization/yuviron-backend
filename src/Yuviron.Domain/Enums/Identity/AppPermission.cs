using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppPermission
{
    AccessAdminPanel,
    ManageCatalog,
    
    PlayerHighQuality,
    PlayerNoAds,
    PrivateSession,    
    CustomTheme,     
    AnimatedMedia,   
    
    AccessBasic,

    StudioArtistManage,
    AccessArtistPanel
}