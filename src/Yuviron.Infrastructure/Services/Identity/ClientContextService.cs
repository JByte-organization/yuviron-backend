using Microsoft.AspNetCore.Http;
using System.Linq;
using UAParser;
using Yuviron.Application.Abstractions.Identity;

namespace Yuviron.Infrastructure.Services;

public class ClientContextService : IClientContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClientContext GetClientContext()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return new ClientContext("Unknown", "Unknown", "Unknown");

        var userAgent = context.Request.Headers.UserAgent.ToString();
        var ip = GetClientIpAddress(context);

        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return new ClientContext("Unknown OS", "Unknown Browser", ip);
        }

        var uaParser = Parser.GetDefault();
        var clientInfo = uaParser.Parse(userAgent);

        string device = clientInfo.OS.Family;
        if (!string.IsNullOrWhiteSpace(clientInfo.OS.Major))
        {
            device += $" {clientInfo.OS.Major}";
        }

        string browser = clientInfo.UA.Family;

        return new ClientContext(
            string.IsNullOrWhiteSpace(device) || device == "Other" ? "Unknown OS" : device,
            string.IsNullOrWhiteSpace(browser) || browser == "Other" ? "Unknown Browser" : browser,
            ip
        );
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedHeader))
        {
            return forwardedHeader.Split(',')[0].Trim();
        }

        var cfHeader = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(cfHeader))
        {
            return cfHeader.Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
    }
}