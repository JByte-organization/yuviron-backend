using Yuviron.Application.Abstractions.Data;
namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IContentContext : IDataContext
{
    System.Linq.IQueryable<Yuviron.Domain.Entities.Banner> Banners { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Lyrics> Lyrics { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.BannerRequest> BannerRequests { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.SmartLink> SmartLinks { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.SmartLinkClick> SmartLinkClicks { get; }
}