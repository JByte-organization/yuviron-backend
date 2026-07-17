using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Yuviron.Application.Abstractions.Identity;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Client.Marketing.Queries.ResolvePublicSmartLink;
using Yuviron.Domain.Enums;

namespace Yuviron.Api.Controllers.Client;

[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class PublicSmartLinksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IClientContextService _clientContextService;
    private readonly string _frontendBaseUrl;

    public PublicSmartLinksController(
        IMediator mediator,
        IClientContextService clientContextService,
        IOptions<FrontendOptions> frontendOptions)
    {
        _mediator = mediator;
        _clientContextService = clientContextService;
        _frontendBaseUrl = frontendOptions.Value.BaseUrl.TrimEnd('/');
    }

    [HttpGet("album/{publicId}")]
    public Task<IActionResult> ResolveAlbum(string publicId, [FromQuery(Name = "si")] string? code, CancellationToken ct)
    {
        return Resolve(SmartLinkType.Album, publicId, code, ct);
    }

    [HttpGet("artist/{publicId}")]
    public Task<IActionResult> ResolveArtist(string publicId, [FromQuery(Name = "si")] string? code, CancellationToken ct)
    {
        return Resolve(SmartLinkType.Artist, publicId, code, ct);
    }

    [HttpGet("track/{publicId}")]
    public Task<IActionResult> ResolveTrack(string publicId, [FromQuery(Name = "si")] string? code, CancellationToken ct)
    {
        return Resolve(SmartLinkType.Track, publicId, code, ct);
    }

    [HttpGet("playlist/{publicId}")]
    public Task<IActionResult> ResolvePlaylist(string publicId, [FromQuery(Name = "si")] string? code, CancellationToken ct)
    {
        return Resolve(SmartLinkType.Playlist, publicId, code, ct);
    }

    [HttpGet("user/{publicId}")]
    public Task<IActionResult> ResolveUserProfile(string publicId, [FromQuery(Name = "si")] string? code, CancellationToken ct)
    {
        return Resolve(SmartLinkType.UserProfile, publicId, code, ct);
    }

    private async Task<IActionResult> Resolve(SmartLinkType entityType, string publicId, string? code, CancellationToken ct)
    {
        var clientContext = _clientContextService.GetClientContext();
        var referrer = Request.Headers.Referer.ToString();

        var result = await _mediator.Send(new ResolvePublicSmartLinkQuery(
            EntityType: entityType,
            PublicId: publicId,
            Code: code,
            CountryCode: null,
            Referrer: string.IsNullOrWhiteSpace(referrer) ? null : referrer,
            DeviceType: clientContext.Device
        ), ct);

        return Redirect($"{_frontendBaseUrl}{result.RelativePath}");
    }
}
