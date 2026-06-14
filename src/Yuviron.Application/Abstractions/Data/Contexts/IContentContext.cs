using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IContentContext : IDataContext
{
    IQueryable<Banner> Banners { get; }
    IQueryable<Lyrics> Lyrics { get; }
    IQueryable<BannerRequest> BannerRequests { get; }
    IQueryable<SmartLink> SmartLinks { get; }
    IQueryable<SmartLinkClick> SmartLinkClicks { get; }
}