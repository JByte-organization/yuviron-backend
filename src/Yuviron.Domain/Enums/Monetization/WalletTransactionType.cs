using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums.Monetization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WalletTransactionType
{
    RoyaltyAccrual, 
    PayoutReserved, 
    PayoutReleased, 
    PayoutCompleted, 
    ManualAdjustment
}