using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Stream.Queries.GetAudioStream;

public sealed class GetAudioStreamHandler : IRequestHandler<GetAudioStreamQuery, GetAudioStreamResponse>
{
    private readonly IFileStorageService _fileStorage;
    private readonly IStreamTokenService _tokenService;

    // Жесткий Allowlist для двойной проверки
    private static readonly string[] AllowedExtensions = { ".m3u8", ".ts", ".key" };

    public GetAudioStreamHandler(IFileStorageService fileStorage, IStreamTokenService tokenService)
    {
        _fileStorage = fileStorage;
        _tokenService = tokenService;
    }

    public async Task<GetAudioStreamResponse> Handle(GetAudioStreamQuery request, CancellationToken cancellationToken)
    {
        if (!_tokenService.ValidateToken(request.TrackId, request.Exp, request.Sig))
        {
            throw new UnauthorizedAccessException("Invalid or expired stream token.");
        }

        var sanitizedFileName = Path.GetFileName(request.FileName);

        var actualFileName = sanitizedFileName.Equals("key", StringComparison.OrdinalIgnoreCase) 
            ? "encryption.key" 
            : sanitizedFileName;

        var extension = Path.GetExtension(actualFileName).ToLowerInvariant();
        if (Array.IndexOf(AllowedExtensions, extension) < 0)
        {
            throw new UnauthorizedAccessException("Forbidden file extension requested.");
        }

        var relativeFilePath = $"tracks/{request.TrackId}/{actualFileName}";
        
        var stream = await _fileStorage.GetFileStreamAsync(relativeFilePath, cancellationToken);

        string contentType = GetContentType(actualFileName);

        if (extension == ".m3u8")
        {
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(cancellationToken);

            var queryParams = $"?exp={request.Exp}&sig={request.Sig}";
        
            var modifiedContent = content.Replace(".ts", $".ts{queryParams}");
            modifiedContent = modifiedContent.Replace("/key\"", $"/key{queryParams}\"");

            var modifiedStream = new MemoryStream(Encoding.UTF8.GetBytes(modifiedContent));
            return new GetAudioStreamResponse(modifiedStream, contentType);
        }

        return new GetAudioStreamResponse(stream, contentType);
    }

    private string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".m3u8" => "application/vnd.apple.mpegurl",
            ".ts" => "video/MP2T",
            ".key" => "application/octet-stream",
            _ => "application/octet-stream"
        };
    }
}