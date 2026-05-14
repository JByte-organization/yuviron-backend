using MediatR;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Utilities;
using Yuviron.Domain.Exceptions;

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
            throw new System.ArgumentException("Invalid file name."); 

        var relativePath = $"temp/{safeFileName}";
        var stream = await _fileStorage.GetFileStreamAsync(relativePath, cancellationToken);

        if (stream == null)
            throw new NotFoundException("TempFile", safeFileName);

        var (_, contentType) = FileSignatureDetector.Detect(stream);

        return new GetTempPreviewResponse(stream, contentType);
    }
    
}