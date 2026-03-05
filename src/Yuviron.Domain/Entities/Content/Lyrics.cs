using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Lyrics : Entity
{
    public Guid TrackId { get; private set; } 
    public string LanguageCode { get; private set; } = "en";
    public string PlainText { get; private set; } = string.Empty;

    public virtual Track Track { get; private set; } = null!;

    private Lyrics() { }

    public static Lyrics Create(Guid trackId, string languageCode, string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText)) throw new ArgumentException("Lyrics text cannot be empty");

        return new Lyrics
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