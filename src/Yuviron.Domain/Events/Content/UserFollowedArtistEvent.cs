namespace Yuviron.Domain.Events;

public record UserFollowedArtistEvent(Guid UserId, Guid ArtistId);