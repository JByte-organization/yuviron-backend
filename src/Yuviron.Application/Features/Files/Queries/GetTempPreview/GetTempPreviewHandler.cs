using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Files.Queries.GetTempPreview;

public sealed class GetTempPreviewHandler : IRequestHandler<GetTempPreviewQuery, GetTempPreviewResponse>
{
    private readonly IFileStorageService _fileStorage;
    private readonly IApplicationDbContext _context;

    public GetTempPreviewHandler(IFileStorageService fileStorage, IApplicationDbContext context)
    {
        _fileStorage = fileStorage;
        _context = context;
    }

    public async Task<GetTempPreviewResponse> Handle(GetTempPreviewQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.FileName, out var fileId))
            throw new NotFoundException("TempFile", request.FileName);

        var fileMeta = await _context.FileMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fileId && f.IsTemporary, cancellationToken);

        if (fileMeta == null)
            throw new NotFoundException("TempFile", request.FileName);

        var stream = await _fileStorage.GetFileStreamAsync(fileMeta.CurrentStorageKey, cancellationToken);

        if (stream == null)
            throw new NotFoundException("TempFile", request.FileName);

        return new GetTempPreviewResponse(stream, fileMeta.ContentType);
    }
}