using Yuviron.Application.Integrations.Jamendo.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.Services;

public interface IJamendoApiService
{
    Task<List<JamendoTrackDto>> GetTracksAsync(int limit = 10, int offset = 0, CancellationToken cancellationToken = default);
    
    Task<string> DownloadFileToTempAsync(string url, string extension, CancellationToken cancellationToken = default);
}