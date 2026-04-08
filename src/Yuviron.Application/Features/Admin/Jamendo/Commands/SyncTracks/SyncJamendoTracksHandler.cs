using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;
using Yuviron.Application.Integrations.Jamendo;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

public sealed class SyncJamendoTracksHandler : IRequestHandler<SyncJamendoTracksCommand, int>
{
    private readonly IJamendoApiService _jamendoApi;
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly ILogger<SyncJamendoTracksHandler> _logger;

    public SyncJamendoTracksHandler(
        IJamendoApiService jamendoApi,
        IApplicationDbContext context,
        ISender sender,
        ILogger<SyncJamendoTracksHandler> logger)
    {
        _jamendoApi = jamendoApi;
        _context = context;
        _sender = sender;
        _logger = logger;
    }

    public async Task<int> Handle(SyncJamendoTracksCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начинаем синхронизацию {Limit} треков из Jamendo...", request.Limit);

        var jamendoTracks = await _jamendoApi.GetPopularTracksAsync(request.Limit, cancellationToken);
        if (!jamendoTracks.Any()) return 0;

        var defaultGenre = await GetOrCreateGenreAsync("Jamendo Hits", cancellationToken);
        var defaultMood = await GetOrCreateMoodAsync("Various", cancellationToken);

        int syncedCount = 0;

        foreach (var jt in jamendoTracks)
        {
            try
            {
                var artist = await GetOrCreateArtistAsync(jt.ArtistName, cancellationToken);

                var trackExists = await _context.Tracks
                    .AnyAsync(t => t.Title == jt.Name && t.TrackArtists.Any(ta => ta.ArtistId == artist.Id), cancellationToken);
                
                if (trackExists)
                {
                    _logger.LogInformation("Трек {TrackName} уже есть в базе. Пропускаем.", jt.Name);
                    continue;
                }

                var album = await GetOrCreateAlbumAsync(jt.AlbumName, artist.Id, cancellationToken);

                _logger.LogInformation("Скачиваем файлы для трека {TrackName}...", jt.Name);
                var audioKey = await _jamendoApi.DownloadFileToTempAsync(jt.AudioDownloadUrl, ".mp3", cancellationToken);
                var coverKey = await _jamendoApi.DownloadFileToTempAsync(jt.CoverUrl, ".jpg", cancellationToken);

                if (string.IsNullOrWhiteSpace(audioKey))
                {
                    _logger.LogWarning("Не удалось скачать аудио для {TrackName}. Пропускаем.", jt.Name);
                    continue;
                }

                // ИСПРАВЛЕНО: CreateTrackCommand теперь вызывается через круглые скобки
                var createTrackCmd = new CreateTrackCommand(
                    album.Id,
                    1,
                    jt.Name,
                    false,
                    audioKey,
                    coverKey,
                    VisibilityStatus.Published, // Приводим 1 к Enum (Published)
                    new List<Guid> { artist.Id },
                    new List<Guid> { defaultGenre.Id },
                    new List<Guid> { defaultMood.Id }
                );

                await _sender.Send(createTrackCmd, cancellationToken);
                syncedCount++;
                
                _logger.LogInformation("Трек {TrackName} успешно отправлен на обработку HLS!", jt.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при синхронизации трека {TrackName}", jt.Name);
            }
        }

        return syncedCount;
    }

    private async Task<Genre> GetOrCreateGenreAsync(string name, CancellationToken cancellationToken)
    {
        var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
        if (genre == null)
        {
            // ИСПРАВЛЕНО: Добавлен null для CoverUrl и DateTime.UtcNow
            genre = Genre.Create(name, null, DateTime.UtcNow);
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return genre;
    }

    private async Task<Mood> GetOrCreateMoodAsync(string name, CancellationToken cancellationToken)
    {
        var mood = await _context.Moods.FirstOrDefaultAsync(m => m.Name == name, cancellationToken);
        if (mood == null)
        {
            // ИСПРАВЛЕНО: Добавлен null для CoverUrl и DateTime.UtcNow
            mood = Mood.Create(name, null, DateTime.UtcNow);
            _context.Moods.Add(mood);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return mood;
    }

    private async Task<Artist> GetOrCreateArtistAsync(string name, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == name, cancellationToken);
        if (artist == null)
        {
            // ИСПРАВЛЕНО: Переданы все 7 параметров
            artist = Artist.Create(
                null, 
                name, 
                "Imported from Jamendo", 
                null, 
                null, 
                default(VerificationStatus), // Статус по умолчанию (например, Unverified)
                DateTime.UtcNow);
                
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return artist;
    }

    private async Task<Album> GetOrCreateAlbumAsync(string title, Guid artistId, CancellationToken cancellationToken)
    {
        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Singles" : title;
        
        var album = await _context.Albums.FirstOrDefaultAsync(a => a.Title == safeTitle && a.AlbumArtists.Any(aa => aa.ArtistId == artistId), cancellationToken);
        if (album == null)
        {
            // ИСПРАВЛЕНО: Переданы все 8 параметров
            album = Album.Create(
                safeTitle, 
                "Imported from Jamendo", 
                null, 
                DateTime.UtcNow, 
                VisibilityStatus.Published,
                null, 
                new List<Guid> { artistId }, 
                DateTime.UtcNow);

            _context.Albums.Add(album);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return album;
    }
}