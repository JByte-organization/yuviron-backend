using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetAdminTrackPreviewUrl;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.StreamAdminTrackAudio;

public sealed class StreamAdminTrackAudioHandler : IRequestHandler<StreamAdminTrackAudioQuery, StreamAdminTrackAudioResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IStreamTokenService _streamTokenService;
    private readonly IFileStorageService _fileStorage;

    public StreamAdminTrackAudioHandler(
        IApplicationDbContext context,
        IStreamTokenService streamTokenService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _streamTokenService = streamTokenService;
        _fileStorage = fileStorage;
    }

    public async Task<StreamAdminTrackAudioResponse> Handle(StreamAdminTrackAudioQuery request, CancellationToken cancellationToken)
    {
        if (!_streamTokenService.ValidateToken(request.TrackId, GetAdminTrackPreviewUrlHandler.PreviewQualitySentinel, request.Exp, request.Uid, request.Sig))
            throw new UnauthorizedAccessException("Invalid or expired admin preview token.");

        var audioKey = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == request.TrackId)
            .Select(t => t.AudioStorageKey)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var stream = await _fileStorage.GetFileStreamAsync(audioKey, cancellationToken)
            ?? throw new NotFoundException("AudioFile", audioKey);

        var contentType = Path.GetExtension(audioKey).ToLowerInvariant() switch
        {
            ".mp3" => "audio/mpeg",
            ".flac" => "audio/flac",
            ".wav" => "audio/wav",
            ".aac" => "audio/aac",
            ".ogg" => "audio/ogg",
            _ => "application/octet-stream"
        };

        return new StreamAdminTrackAudioResponse(stream, contentType);
    }
}
