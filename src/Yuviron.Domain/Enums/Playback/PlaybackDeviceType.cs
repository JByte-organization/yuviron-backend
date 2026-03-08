using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlaybackDeviceType
{
    Unknown = 0,
    WebPlayer = 1,
    IosApp = 2,
    AndroidApp = 3,
    DesktopApp = 4,
    SmartSpeaker = 5
}