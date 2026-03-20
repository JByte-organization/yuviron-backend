using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Yuviron.Application.Abstractions.Authentication;

namespace Yuviron.Infrastructure.Authentication;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub");
            return idClaim != null && Guid.TryParse(idClaim.Value, out var id) ? id : Guid.Empty;
        }
    }

    public bool HasPermission(string permission)
    {
        return _httpContextAccessor.HttpContext?.User
            .HasClaim(c => c.Type == "permission" && c.Value == permission) ?? false;
    }
}