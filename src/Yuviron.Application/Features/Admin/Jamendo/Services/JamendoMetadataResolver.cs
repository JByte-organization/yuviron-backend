using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services.Jamendo;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Jamendo.Services;

public class JamendoMetadataResolver : IJamendoMetadataResolver
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<string, Guid> _genreCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Guid> _moodCache = new(StringComparer.OrdinalIgnoreCase);

    public JamendoMetadataResolver(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<Guid>> ResolveGenresAsync(IEnumerable<string>? genreNames, CancellationToken cancellationToken)
    {
        var resolvedIds = new List<Guid>();
        var namesToProcess = (genreNames != null && genreNames.Any()) ? genreNames : new[] { "Unknown" };

        foreach (var name in namesToProcess)
        {
            var formattedName = FormatName(name);
            if (!_genreCache.TryGetValue(formattedName, out var id))
            {
                id = await GetOrCreateGenreAsync(formattedName, cancellationToken);
                _genreCache[formattedName] = id;
            }
            resolvedIds.Add(id);
        }
        return resolvedIds;
    }

    public async Task<List<Guid>> ResolveMoodsAsync(IEnumerable<string>? moodNames, CancellationToken cancellationToken)
    {
        var resolvedIds = new List<Guid>();
        var namesToProcess = (moodNames != null && moodNames.Any()) ? moodNames : new[] { "Various" };

        foreach (var name in namesToProcess)
        {
            var formattedName = FormatName(name);
            if (!_moodCache.TryGetValue(formattedName, out var id))
            {
                id = await GetOrCreateMoodAsync(formattedName, cancellationToken);
                _moodCache[formattedName] = id;
            }
            resolvedIds.Add(id);
        }
        return resolvedIds;
    }

    private async Task<Guid> GetOrCreateGenreAsync(string name, CancellationToken ct)
    {
        var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == name, ct);
        if (genre == null)
        {
            genre = Genre.Create(name, null, _timeProvider.GetUtcNow().UtcDateTime);
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync(ct); 
        }
        return genre.Id;
    }

    private async Task<Guid> GetOrCreateMoodAsync(string name, CancellationToken ct)
    {
        var mood = await _context.Moods.FirstOrDefaultAsync(m => m.Name == name, ct);
        if (mood == null)
        {
            mood = Mood.Create(name, null, _timeProvider.GetUtcNow().UtcDateTime);
            _context.Moods.Add(mood);
            await _context.SaveChangesAsync(ct);
        }
        return mood.Id;
    }

    private static string FormatName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name;
        return char.ToUpper(name[0]) + name.Substring(1).ToLower();
    }
}