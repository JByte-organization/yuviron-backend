using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Files.Queries.GetTempPreview;

public sealed class GetTempPreviewHandler : IRequestHandler<GetTempPreviewQuery, GetTempPreviewResponse>
{
    private readonly IFileStorageService _fileStorage;
    private readonly ISystemContext _systemContext;

    public GetTempPreviewHandler(IFileStorageService fileStorage, ISystemContext systemContext)
    {
        _fileStorage = fileStorage;
        _systemContext = systemContext;
    }

    public async Task<GetTempPreviewResponse> Handle(GetTempPreviewQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.FileName, out var fileId))
            throw new NotFoundException("File", request.FileName);

        var fileMeta = await _systemContext.FileMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(f => 
                f.Id == fileId && 
                f.IsTemporary && 
                f.UserId == request.UserId, cancellationToken);

        if (fileMeta == null)
            throw new NotFoundException("File", request.FileName);

        var stream = await _fileStorage.GetFileStreamAsync(fileMeta.CurrentStorageKey, cancellationToken);

        if (stream == null)
            throw new NotFoundException("File", request.FileName);

        return new GetTempPreviewResponse(stream, fileMeta.ContentType);
    }
}