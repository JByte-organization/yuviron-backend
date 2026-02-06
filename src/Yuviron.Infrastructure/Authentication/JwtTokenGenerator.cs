using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Constants;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.UserRoles != null)
        {
            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));

                if (userRole.Role.RolePermissions != null)
                {
                    foreach (var rp in userRole.Role.RolePermissions)
                    {
                        claims.Add(new Claim("permission", rp.Permission.Name));
                    }
                }
            }
        }

        if (user.Subscriptions != null)
        {
            var isPremium = user.Subscriptions.Any(s =>
                s.Status == SubscriptionStatus.Active &&
                s.EndAt > DateTime.UtcNow);

            if (isPremium)
            {
                claims.Add(new Claim("is_premium", "true"));

                var premiumPermissions = new[]
                {
                    Perms.HighQualityAudio,
                    Perms.NoAds,
                };

                foreach (var perm in premiumPermissions)
                {
                    if (!claims.Any(c => c.Type == "permission" && c.Value == perm))
                    {
                        claims.Add(new Claim("permission", perm));
                    }
                }
            }
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}