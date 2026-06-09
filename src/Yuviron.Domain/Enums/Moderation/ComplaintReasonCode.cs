using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComplaintReasonCode
{
    Unknown = 0,
    Spam = 1,
    CopyrightViolation = 2,
    Pornography = 3,
    HateSpeech = 4,
    Abuse = 5,
    Scam = 6,
    Other = 99
}
