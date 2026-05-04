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

        var actualFileName = request.FileName.Equals("key", StringComparison.OrdinalIgnoreCase) 
            ? "encryption.key" 
            : request.FileName;

        var relativeFilePath = $"tracks/{request.TrackId}/{actualFileName}";
        var stream = await _fileStorage.GetFileStreamAsync(relativeFilePath, cancellationToken);

        string contentType = GetContentType(actualFileName);

        if (request.FileName.EndsWith(".m3u8"))
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
        if (fileName.EndsWith(".m3u8")) return "application/vnd.apple.mpegurl";
        if (fileName.EndsWith(".ts")) return "video/MP2T";
        if (fileName.EndsWith(".key")) return "application/octet-stream"; 
        if (fileName.EndsWith(".mp3")) return "audio/mpeg";
        
        return "application/octet-stream";
    }
}