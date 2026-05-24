using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed class UploadFileHandler : IRequestHandler<UploadFileCommand, UploadResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public UploadFileHandler(
        IApplicationDbContext context, 
        IFileStorageService fileStorageService, 
        ICurrentUserService currentUser,
        TimeProvider timeProvider)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<UploadResponse> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var fileId = Guid.NewGuid(); 
        var uniqueFileName = fileId.ToString("N");
        var targetFolder = "temp";

        var filePath = await _fileStorageService.UploadAsync(
            request.FileStream, targetFolder, uniqueFileName, request.ContentType, cancellationToken);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var fileMeta = FileMetadata.Create(
            fileId, userId, request.FileName, request.ContentType, request.FileStream.Length, filePath, utcNow);
    
        _context.FileMetadata.Add(fileMeta);
        await _context.SaveChangesAsync(cancellationToken);
    
        return new UploadResponse(fileId, $"/api/files/temp/{uniqueFileName}");
    }
}