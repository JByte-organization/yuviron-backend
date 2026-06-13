using System;
using System.Collections.Generic;
using System.Linq;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Extensions;

public static class ArtistTeamMemberExtensions
{
    /// <summary>
    /// Owner only access (Delete artist account).
    /// </summary>
    public static IQueryable<ArtistTeamMember> HasFullAccess(
        this IQueryable<ArtistTeamMember> query, 
        Guid artistId, 
        Guid userId)
    {
        return query.Where(tm => 
            tm.ArtistId == artistId && 
            tm.UserId == userId && 
            tm.Role == ArtistTeamRole.Owner);
    }

    /// <summary>
    /// Owner or Manager access (Albums, Tracks, Playlists, Marketing).
    /// </summary>
    public static IQueryable<ArtistTeamMember> HasManagementAccess(
        this IQueryable<ArtistTeamMember> query, 
        Guid artistId, 
        Guid userId)
    {
        return query.Where(tm => 
            tm.ArtistId == artistId && 
            tm.UserId == userId && 
            (tm.Role == ArtistTeamRole.Owner || tm.Role == ArtistTeamRole.Manager));
    }

    /// <summary>
    /// For multi-artist context (e.g. tracks with multiple artists).
    /// </summary>
    public static IQueryable<ArtistTeamMember> HasManagementAccess(
        this IQueryable<ArtistTeamMember> query, 
        IEnumerable<Guid> artistIds, 
        Guid userId)
    {
        return query.Where(tm => 
            artistIds.Contains(tm.ArtistId) && 
            tm.UserId == userId && 
            (tm.Role == ArtistTeamRole.Owner || tm.Role == ArtistTeamRole.Manager));
    }

    /// <summary>
    /// Owner, Manager or Editor access (Profile, Photos, Social Links).
    /// </summary>
    public static IQueryable<ArtistTeamMember> HasEditorAccess(
        this IQueryable<ArtistTeamMember> query, 
        Guid artistId, 
        Guid userId)
    {
        return query.Where(tm => 
            tm.ArtistId == artistId && 
            tm.UserId == userId && 
            (tm.Role == ArtistTeamRole.Owner || tm.Role == ArtistTeamRole.Manager || tm.Role == ArtistTeamRole.Editor));
    }

    /// <summary>
    /// Owner, Manager, Editor or Viewer access (Analytics, Stats, Reading data).
    /// </summary>
    public static IQueryable<ArtistTeamMember> HasViewerAccess(
        this IQueryable<ArtistTeamMember> query, 
        Guid artistId, 
        Guid userId)
    {
        return query.Where(tm => 
            tm.ArtistId == artistId && 
            tm.UserId == userId && 
            (tm.Role == ArtistTeamRole.Owner || tm.Role == ArtistTeamRole.Manager || 
             tm.Role == ArtistTeamRole.Editor || tm.Role == ArtistTeamRole.Viewer));
    }
}
