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
    private readonly string _baseUrl;

    public JamendoApiService(
        HttpClient httpClient,
        IConfiguration configuration,
        IFileStorageService fileStorageService,
        ILogger<JamendoApiService> logger)
    {
        _httpClient = httpClient;
        
        _baseUrl = (configuration["JamendoApi:BaseUrl"] ?? "https://api.jamendo.com/v3.0").TrimEnd('/');
        
        _clientId = configuration["JamendoApi:ClientId"] 
                    ?? throw new ArgumentNullException("Jamendo ClientId is missing in appsettings.json");
        
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<List<JamendoTrackDto>> GetTracksAsync(int limit = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}/tracks/?client_id={_clientId}&format=json&limit={limit}&offset={offset}&order=releasedate_desc&hasimage=true&audiodlformat=mp32";
        
        _logger.LogInformation("Request {Limit} of tracks (offset {Offset}) from Jamendo API", limit, offset);
        
        try
        {
            var rawJson = await _httpClient.GetStringAsync(url, cancellationToken);

            var options = new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };

            var response = System.Text.Json.JsonSerializer.Deserialize<JamendoResponse>(rawJson, options);

            if (response == null || response.Results == null || response.Results.Count == 0)
            {
                _logger.LogWarning("Jamendo API returned an empty track list. Status: {Status}", response?.Headers?.Status ?? "NULL");
                return new List<JamendoTrackDto>();
            }

            return response.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when accessing Jamendo API");
            return new List<JamendoTrackDto>();
        }
    }

    public async Task<string> DownloadFileToTempAsync(string fileUrl, string extension, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return string.Empty;

        _logger.LogInformation("Download the file from Jamendo: {Url}", fileUrl);

        try 
        {
            var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            
            var tempKey = await _fileStorageService.UploadAsync(stream, "temp", $"file{extension}", contentType, cancellationToken);
            
            return tempKey; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from link {Url}", fileUrl);
            return string.Empty;
        }
    }
}