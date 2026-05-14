using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Utilities; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Files.Queries.GetImage;

public sealed class GetImageHandler : IRequestHandler<GetImageQuery, GetImageResponse>
{
    private readonly IFileStorageService _fileStorage;
    private readonly string[] _publicFolders = { "avatars", "covers", "banners" };

    public GetImageHandler(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<GetImageResponse> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        Stream? fileStream = null;

        foreach (var folder in _publicFolders)
        {
            var potentialPath = $"{folder}/{request.Hash}";
            fileStream = await _fileStorage.GetFileStreamAsync(potentialPath, cancellationToken);
            
            if (fileStream != null) break; 
        }

        
        if (fileStream == null)
        {
            throw new NotFoundException("Image", request.Hash);
        }

        var (_, contentType) = FileSignatureDetector.Detect(fileStream);
        fileStream.Position = 0; 

        return new GetImageResponse(fileStream, contentType);
    }
}