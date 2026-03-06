using System;

namespace Yuviron.Application.Common;

public static class ValidationExtensions
{
    public static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var outUri) 
               && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }
}