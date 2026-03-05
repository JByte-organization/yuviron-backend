using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppPermission
{
    // Админка
    AccessAdminPanel,
    ManageUsers,
    ViewSystemLogs,

    // Треки
    TracksUpload,
    TracksEdit,
    TracksDelete,
    TracksBlock,
    CreatePlaylist,

    // Плеер
    PlayerHighQuality,
    PlayerNoAds,

    // Аналитика
    AnalyticsView
}