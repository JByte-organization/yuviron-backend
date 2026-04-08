using Yuviron.Application.Integrations.Jamendo.Models;

namespace Yuviron.Application.Abstractions.Services;

public interface IJamendoApiService
{
    // Получить список треков
    Task<List<JamendoTrackDto>> GetPopularTracksAsync(int limit = 10, CancellationToken cancellationToken = default);
    
    // Скачать файл (картинку или mp3) в нашу временную папку temp/
    Task<string> DownloadFileToTempAsync(string url, string extension, CancellationToken cancellationToken = default);
}