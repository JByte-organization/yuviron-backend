using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.Services;

public interface IHlsTranscodingService
{
    Task<string> TranscodeToHlsAsync(string inputStorageKey, string trackIdStr, CancellationToken cancellationToken = default);
}