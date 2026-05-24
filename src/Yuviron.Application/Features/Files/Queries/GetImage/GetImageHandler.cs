using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Files.Queries.GetImage;

public sealed class GetImageHandler : IRequestHandler<GetImageQuery, GetImageResponse>
{
    private readonly IFileStorageService _fileStorage;
    private readonly IApplicationDbContext _context;

    public GetImageHandler(IFileStorageService fileStorage, IApplicationDbContext context)
    {
        _fileStorage = fileStorage;
        _context = context;
    }

    public async Task<GetImageResponse> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Hash, out var fileId))
            throw new NotFoundException("Image", request.Hash);

        var fileMeta = await _context.FileMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsTemporary, cancellationToken);

        if (fileMeta == null)
            throw new NotFoundException("Image", request.Hash);

        var fileStream = await _fileStorage.GetFileStreamAsync(fileMeta.CurrentStorageKey, cancellationToken);
        
        if (fileStream == null)
            throw new NotFoundException("Image", request.Hash);

        return new GetImageResponse(fileStream, fileMeta.ContentType);
    }
}