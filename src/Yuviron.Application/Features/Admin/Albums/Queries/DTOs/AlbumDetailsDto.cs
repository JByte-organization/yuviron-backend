using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Queries.DTOs;

public record AlbumDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    DateTime ReleaseDate,
    VisibilityStatus VisibilityStatus,
    DateTime? ScheduledPublishAt,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<Guid> ArtistIds // Отдаем айдишники, чтобы фронт мог предзаполнить Select/Dropdown
);