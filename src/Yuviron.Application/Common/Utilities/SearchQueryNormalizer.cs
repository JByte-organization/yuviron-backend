using System.Text.RegularExpressions;

namespace Yuviron.Application.Common.Utilities;

public static partial class SearchQueryNormalizer
{
    public static string Normalize(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        return WhitespaceRegex()
            .Replace(query.Trim(), " ")
            .ToLowerInvariant();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
