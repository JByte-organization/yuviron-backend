using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed class UploadFileHandler : IRequestHandler<UploadFileCommand, UploadResponse>
{
    private readonly IFileStorageService _fileStorageService;

    public UploadFileHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<UploadResponse> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var targetFolder = "temp"; 
        
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var filePath = await _fileStorageService.UploadAsync(
            request.FileStream,
            targetFolder, 
            uniqueFileName,
            request.ContentType,
            cancellationToken
        );

        var savedFileName = Path.GetFileName(filePath);
        
        return new UploadResponse(
            Path: filePath,
            Url: $"/api/files/temp/{savedFileName}" 
        );
    }
}