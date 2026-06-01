using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationCategory
{
    System = 1,      // Системные (модерация, безопасность)
    Music = 2,       // Музыкальные (новые релизы, треки)
    Social = 3,      // Социальные (подписки, комнаты)
    Billing = 4      // Финансовые (подписки, выплаты)
}