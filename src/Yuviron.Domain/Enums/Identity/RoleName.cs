using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoleName
{
    Unknown = 0,
    User = 1,
    ManagementUser = 2,
    Admin = 3
}