using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options; // <-- Для IOptions
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration; // <-- Твой конфиг
using Yuviron.Application.Extensions; // <-- Твой экстеншен безопасности
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Files.Queries.GetImage;

public sealed class GetPublicFileHandler : IRequestHandler<GetPublicFileQuery, GetPublicFileResponse>
{
    private readonly IFileStorageService _fileStorage;
    private readonly IApplicationDbContext _context;
    
    private readonly string[] _publicFolders; // <-- Поле для списка

    public GetPublicFileHandler(
        IFileStorageService fileStorage, 
        IApplicationDbContext context,
        IOptions<FileAccessOptions> accessOptions) // <-- Инжектим настройки
    {
        _fileStorage = fileStorage;
        _context = context;
        _publicFolders = accessOptions.Value.PublicFolders; 
    }

    public async Task<GetPublicFileResponse> Handle(GetPublicFileQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Hash, out var fileId))
            throw new NotFoundException("Image", request.Hash);

        var fileMeta = await _context.FileMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsTemporary, cancellationToken);

        if (fileMeta == null)
            throw new NotFoundException("Image", request.Hash);

        // 🛡️ Идеально чистая и абстрагированная проверка безопасности 🛡️
        if (!fileMeta.CurrentStorageKey.IsPublicResource(_publicFolders))
        {
            throw new UnauthorizedAccessException("Direct access to this resource type is forbidden.");
        }

        var fileStream = await _fileStorage.GetFileStreamAsync(fileMeta.CurrentStorageKey, cancellationToken);
        
        if (fileStream == null)
            throw new NotFoundException("Image", request.Hash);

        return new GetPublicFileResponse(fileStream, fileMeta.ContentType);
    }
}