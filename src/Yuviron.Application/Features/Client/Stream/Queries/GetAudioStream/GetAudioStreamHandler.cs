using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Stream.Queries.GetAudioStream;

public sealed class GetAudioStreamHandler : IRequestHandler<GetAudioStreamQuery, GetAudioStreamResponse>
{
    private readonly IFileStorageService _fileStorage;
    private readonly IStreamTokenService _tokenService;

    private static readonly string[] AllowedExtensions = { ".m3u8", ".ts", ".key", ".mp3", ".wav", ".flac", "" };

    public GetAudioStreamHandler(IFileStorageService fileStorage, IStreamTokenService tokenService)
    {
        _fileStorage = fileStorage;
        _tokenService = tokenService;
    }

    public async Task<GetAudioStreamResponse> Handle(GetAudioStreamQuery request, CancellationToken cancellationToken)
    {
        if (!_tokenService.ValidateToken(request.TrackId, request.Quality, request.Exp, request.Uid, request.Sig))
        {
            throw new UnauthorizedAccessException("Invalid, expired, or stolen stream token.");
        }

        var sanitizedFileName = Path.GetFileName(request.FileName);
        var actualFileName = sanitizedFileName.Equals("key", StringComparison.OrdinalIgnoreCase) 
            ? "encryption.key" : sanitizedFileName;

        var extension = Path.GetExtension(actualFileName).ToLowerInvariant();
        if (Array.IndexOf(AllowedExtensions, extension) < 0)
            throw new UnauthorizedAccessException("Forbidden file extension requested.");

        var relativeFilePath = $"tracks/{request.TrackId}/{request.Quality}/{actualFileName}";
        
        var stream = await _fileStorage.GetFileStreamAsync(relativeFilePath, cancellationToken);
        
        if (stream == null)
            throw new NotFoundException("StreamFile", actualFileName);

        string contentType = GetContentType(actualFileName);

        if (extension == ".m3u8")
        {
            using (stream)
            using (var reader = new StreamReader(stream))
            {
                var sb = new StringBuilder();
                string? line;
                
                var queryParams = $"?exp={request.Exp}&uid={request.Uid:N}&sig={request.Sig}";

                while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
                {
                    if (line.StartsWith("#EXT-X-KEY:", StringComparison.OrdinalIgnoreCase))
                    {
                        int lastQuoteIndex = line.LastIndexOf('"');
                        if (lastQuoteIndex > 0)
                        {
                            line = line.Insert(lastQuoteIndex, queryParams);
                        }
                    }
                    else if (!line.StartsWith("#") && line.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
                    {
                        line = $"{line}{queryParams}";
                    }
    
                    sb.AppendLine(line);
                }

                var modifiedStream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
                return new GetAudioStreamResponse(modifiedStream, contentType);
            }
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
            "" => "application/octet-stream",
            _ => "application/octet-stream"
        };
    }
}