using System;
using System.Collections.Generic;
using System.Linq;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Extensions;

public static class ArtistTeamMemberExtensions
{
    /// <summary>
    /// Перевіряє, чи має користувач права на управління конкретним артистом (Owner або Manager).
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
    /// Перевіряє, чи має користувач права на управління хоча б одним артистом зі списку (Owner або Manager).
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
}