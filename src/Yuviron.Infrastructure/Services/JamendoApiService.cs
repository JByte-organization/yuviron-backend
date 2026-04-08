using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Integrations.Jamendo.Models;

namespace Yuviron.Infrastructure.Services;

public class JamendoApiService : IJamendoApiService
{
    private readonly HttpClient _httpClient;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<JamendoApiService> _logger;
    private readonly string _clientId;
    private readonly string _baseUrl; // <-- Вот оно, наше новое поле!

    public JamendoApiService(
        HttpClient httpClient,
        IConfiguration configuration,
        IFileStorageService fileStorageService,
        ILogger<JamendoApiService> logger)
    {
        _httpClient = httpClient;
        
        // Надежно читаем URL из конфига, отрезая слэш на конце
        _baseUrl = (configuration["JamendoApi:BaseUrl"] ?? "https://api.jamendo.com/v3.0").TrimEnd('/');
        
        _clientId = configuration["JamendoApi:ClientId"] 
                    ?? throw new ArgumentNullException("Jamendo ClientId is missing in appsettings.json");
        
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<List<JamendoTrackDto>> GetPopularTracksAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        // Клеим идеальный URL
        var url = $"{_baseUrl}/tracks/?client_id={_clientId}&format=json&limit={limit}&order=popularity_total&hasimage=true&audiodlformat=mp32";
        
        _logger.LogInformation("Запрашиваем {Limit} треков из Jamendo API", limit);
        
        try
        {
            // Читаем сырой JSON
            var rawJson = await _httpClient.GetStringAsync(url, cancellationToken);

            _logger.LogInformation("Сырой JSON от Jamendo: {Json}", rawJson);
            
            // Игнорируем регистр (status vs Status)
            var options = new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };

            // Распаковываем
            var response = System.Text.Json.JsonSerializer.Deserialize<JamendoResponse>(rawJson, options);

            if (response == null || response.Results == null || response.Results.Count == 0)
            {
                _logger.LogWarning("Jamendo API вернул пустой список треков. Статус: {Status}", response?.Headers?.Status ?? "NULL");
                return new List<JamendoTrackDto>();
            }

            return response.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обращении к Jamendo API");
            return new List<JamendoTrackDto>();
        }
    }

    public async Task<string> DownloadFileToTempAsync(string fileUrl, string extension, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return string.Empty;

        _logger.LogInformation("Скачиваем файл с Jamendo: {Url}", fileUrl);

        var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        var tempKey = await _fileStorageService.UploadAsync(stream, "temp", uniqueFileName, contentType, cancellationToken);
        
        return tempKey; 
    }
}