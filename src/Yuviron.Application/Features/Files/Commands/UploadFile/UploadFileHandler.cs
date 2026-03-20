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
        var filePath = await _fileStorageService.UploadAsync(
            request.FileStream,
            "temp", 
            request.FileName,
            request.ContentType,
            cancellationToken
        );

        return new UploadResponse(
            Path: filePath,
            Url: $"/storage/{filePath}"
        );
    }
}