using Yuviron.Application.Abstractions.Data;
namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IIdentityContext : IDataContext
{
    System.Linq.IQueryable<Yuviron.Domain.Entities.User> Users { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Role> Roles { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserRole> UserRoles { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.RefreshToken> RefreshTokens { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserBlock> UserBlocks { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Permission> Permissions { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.RolePermission> RolePermissions { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserDevice> UserDevices { get; }
}