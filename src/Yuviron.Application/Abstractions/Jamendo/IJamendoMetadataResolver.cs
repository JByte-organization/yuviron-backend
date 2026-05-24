
namespace Yuviron.Application.Abstractions.Services.Jamendo;

public interface IJamendoMetadataResolver
{
    Task<List<Guid>> ResolveGenresAsync(IEnumerable<string>? genreNames, CancellationToken cancellationToken);
    Task<List<Guid>> ResolveMoodsAsync(IEnumerable<string>? moodNames, CancellationToken cancellationToken);
}