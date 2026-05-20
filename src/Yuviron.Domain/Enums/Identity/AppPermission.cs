using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppPermission
{
    AccessAdminPanel,
    ManageUsers,
    ManageCatalog,
    ViewSystemLogs,

    TracksUpload,
    TracksEdit,
    TracksDelete,
    TracksBlock,
    CreatePlaylist,

    PlayerHighQuality,
    PlayerNoAds,
    PrivateSession,    
    CustomTheme,     
    AnimatedMedia,     

    StudioArtistProfile,
    AnalyticsView
}