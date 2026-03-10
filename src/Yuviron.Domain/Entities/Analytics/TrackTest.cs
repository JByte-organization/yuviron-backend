using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class TrackTest : Entity
{
    public Guid TrackId { get; private set; } 
    public string LanguageCode { get; private set; } = "ua";
    public string PlainText { get; private set; } = string.Empty;

    public virtual Track Track { get; private set; } = null!;

    private TrackTest() { }

    public static TrackTest Create(Guid trackId, string languageCode, string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText)) throw new ArgumentException("TrackTest  cannot be empty");

        return new TrackTest
        {
            Id = Guid.NewGuid(),
            TrackId = trackId,
            LanguageCode = languageCode.ToLower().Trim(),
            PlainText = plainText.Trim()
        };
    }

    public void UpdateText(string plainText)
    {
        PlainText = plainText.Trim();
    }
}

