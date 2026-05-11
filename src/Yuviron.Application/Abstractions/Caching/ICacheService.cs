namespace Yuviron.Application.Abstractions.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> SortedSetAddAsync(string key, string member, double score, CancellationToken cancellationToken = default);
    
    Task<bool> SortedSetRemoveAsync(string key, string member, CancellationToken cancellationToken = default);
    
    Task<string[]> SortedSetRangeByRankAsync(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default);
    
    Task<long> SortedSetLengthAsync(string key, CancellationToken cancellationToken = default);
    
    Task<double?> SortedSetScoreAsync(string key, string member, CancellationToken cancellationToken = default);
    
    Task<bool> SetAddAsync(string key, string member, CancellationToken cancellationToken = default);
    
    Task<long> SortedSetAddManyAsync(string key, Dictionary<string, double> membersAndScores, CancellationToken cancellationToken = default);
    
    Task<Dictionary<string, double>> SortedSetRangeByRankWithScoresAsync(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default);
}