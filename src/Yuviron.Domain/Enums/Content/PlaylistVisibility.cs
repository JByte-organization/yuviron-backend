using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlaylistVisibility
{
    Private = 0,  // Видит только создатель
    Public = 1,   // Видят все, отображается в поиске
    Unlisted = 2  // Доступ только по прямой ссылке (скрыт из поиска)
}