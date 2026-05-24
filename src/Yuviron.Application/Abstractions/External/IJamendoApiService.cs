using Yuviron.Application.Integrations.Jamendo.Models;

namespace Yuviron.Application.Abstractions.Services;

public record JamendoDownloadedFile(Guid FileId, string TempKey, string ContentType, long SizeBytes);

public interface IJamendoApiService
{
    Task<List<JamendoTrackDto>> GetTracksAsync(int limit = 10, int offset = 0, CancellationToken cancellationToken = default);
    
    Task<JamendoDownloadedFile?> DownloadFileToTempAsync(string fileUrl, string extension, CancellationToken cancellationToken = default);
}