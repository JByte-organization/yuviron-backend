namespace Yuviron.Application.Common.Utilities;

public static class SearchFuzzyMatcher
{
    public static bool ShouldUseFuzzy(string normalizedQuery)
    {
        return SplitTokens(normalizedQuery).Any(token => token.Length >= 4);
    }

    public static IReadOnlyCollection<string> BuildCandidateFragments(string normalizedQuery)
    {
        if (!ShouldUseFuzzy(normalizedQuery))
        {
            return Array.Empty<string>();
        }

        var fragments = new HashSet<string>(StringComparer.Ordinal);

        foreach (var token in SplitTokens(normalizedQuery))
        {
            if (token.Length < 3)
            {
                continue;
            }

            for (var index = 0; index <= token.Length - 3; index++)
            {
                fragments.Add(token.Substring(index, 3));
            }
        }

        return fragments.ToArray();
    }

    public static bool TryGetScore(
        string normalizedQuery,
        IEnumerable<string?> searchableValues,
        out double score)
    {
        score = 0;

        if (!ShouldUseFuzzy(normalizedQuery))
        {
            return false;
        }

        foreach (var searchableValue in searchableValues)
        {
            var normalizedValue = SearchQueryNormalizer.Normalize(searchableValue);
            if (normalizedValue.Length == 0)
            {
                continue;
            }

            score = Math.Max(score, CalculateScore(normalizedQuery, normalizedValue));
        }

        return score >= GetAcceptanceThreshold(normalizedQuery);
    }

    private static double CalculateScore(string normalizedQuery, string normalizedValue)
    {
        if (normalizedValue.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 1d;
        }

        var fullSimilarity = GetNormalizedSimilarity(normalizedQuery, normalizedValue);
        var tokenSimilarity = GetTokenSimilarity(normalizedQuery, normalizedValue);

        if (normalizedValue.StartsWith(normalizedQuery, StringComparison.Ordinal) ||
            normalizedQuery.StartsWith(normalizedValue, StringComparison.Ordinal))
        {
            fullSimilarity = Math.Min(fullSimilarity + 0.05d, 1d);
        }

        return Math.Max(fullSimilarity, tokenSimilarity);
    }

    private static double GetTokenSimilarity(string normalizedQuery, string normalizedValue)
    {
        var queryTokens = SplitTokens(normalizedQuery);
        var valueTokens = SplitTokens(normalizedValue);

        if (queryTokens.Length == 0 || valueTokens.Length == 0)
        {
            return 0d;
        }

        var totalScore = 0d;

        foreach (var queryToken in queryTokens)
        {
            var bestTokenScore = 0d;

            foreach (var valueToken in valueTokens)
            {
                bestTokenScore = Math.Max(bestTokenScore, GetNormalizedSimilarity(queryToken, valueToken));

                if (bestTokenScore >= 1d)
                {
                    break;
                }
            }

            totalScore += bestTokenScore;
        }

        return totalScore / queryTokens.Length;
    }

    private static double GetAcceptanceThreshold(string normalizedQuery)
    {
        var effectiveLength = normalizedQuery.Replace(" ", string.Empty, StringComparison.Ordinal).Length;

        if (effectiveLength <= 5)
        {
            return 0.83d;
        }

        if (effectiveLength <= 8)
        {
            return 0.75d;
        }

        return 0.68d;
    }

    private static double GetNormalizedSimilarity(string left, string right)
    {
        var maxLength = Math.Max(left.Length, right.Length);
        if (maxLength == 0)
        {
            return 1d;
        }

        var distance = GetDamerauLevenshteinDistance(left, right);
        return 1d - distance / (double)maxLength;
    }

    private static int GetDamerauLevenshteinDistance(string source, string target)
    {
        var rows = source.Length + 1;
        var columns = target.Length + 1;
        var distances = new int[rows, columns];

        for (var row = 0; row < rows; row++)
        {
            distances[row, 0] = row;
        }

        for (var column = 0; column < columns; column++)
        {
            distances[0, column] = column;
        }

        for (var row = 1; row < rows; row++)
        {
            for (var column = 1; column < columns; column++)
            {
                var substitutionCost = source[row - 1] == target[column - 1] ? 0 : 1;

                distances[row, column] = Math.Min(
                    Math.Min(
                        distances[row - 1, column] + 1,
                        distances[row, column - 1] + 1),
                    distances[row - 1, column - 1] + substitutionCost);

                if (row > 1 &&
                    column > 1 &&
                    source[row - 1] == target[column - 2] &&
                    source[row - 2] == target[column - 1])
                {
                    distances[row, column] = Math.Min(distances[row, column], distances[row - 2, column - 2] + 1);
                }
            }
        }

        return distances[rows - 1, columns - 1];
    }

    private static string[] SplitTokens(string normalizedQuery)
    {
        return normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
