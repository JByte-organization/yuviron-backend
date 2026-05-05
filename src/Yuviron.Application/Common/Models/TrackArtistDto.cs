using Yuviron.Domain.Enums;

namespace Yuviron.Application.Common.Models;

public record TrackArtistDto(
    Guid Id, 
    string Name, 
    ArtistRole Role 
);