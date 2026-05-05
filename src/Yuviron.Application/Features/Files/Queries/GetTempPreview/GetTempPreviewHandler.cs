using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Files.Queries.GetTempPreview;

public sealed class GetTempPreviewHandler : IRequestHandler<GetTempPreviewQuery, GetTempPreviewResponse>
{
    private readonly IFileStorageService _fileStorage;

    public GetTempPreviewHandler(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<GetTempPreviewResponse> Handle(GetTempPreviewQuery request, CancellationToken cancellationToken)
    {
        var safeFileName = Path.GetFileName(request.FileName);

        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new System.ArgumentException("Invalid file name.");
        }

        var relativePath = $"temp/{safeFileName}";
        var stream = await _fileStorage.GetFileStreamAsync(relativePath, cancellationToken);

        var ext = Path.GetExtension(safeFileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            _ => "application/octet-stream"
        };

        return new GetTempPreviewResponse(stream, contentType);
    }
}