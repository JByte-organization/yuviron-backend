using System;
using System.Linq;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Extensions;

public static class ContentVisibilityExtensions
{
    public static IQueryable<Album> AvailableForPublic(this IQueryable<Album> query, DateTime utcNow)
    {
        return query.Where(a => 
            !a.IsDeleted && 
            a.VisibilityStatus == VisibilityStatus.Published && 
            a.ReleaseDate <= utcNow);
    }

    public static IQueryable<Track> AvailableForPublic(this IQueryable<Track> query, DateTime utcNow)
    {
        return query.Where(t => 
            !t.IsDeleted &&
            t.VisibilityStatus == VisibilityStatus.Published &&
            t.ProcessingStatus == TrackProcessingStatus.Ready &&
            t.Album != null &&
            !t.Album.IsDeleted &&
            t.Album.VisibilityStatus == VisibilityStatus.Published &&
            t.Album.ReleaseDate <= utcNow);
    }
}