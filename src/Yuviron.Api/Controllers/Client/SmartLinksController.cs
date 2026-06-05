using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Yuviron.Application.Abstractions.Identity;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Client.Marketing.Queries.ResolveSmartLink;

namespace Yuviron.Api.Controllers.Client;

[AllowAnonymous] 
[Route("sl")] 
[ApiExplorerSettings(IgnoreApi = true)] 
public class SmartLinksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IClientContextService _clientContextService;
    private readonly string _defaultFrontendUrl;
    private readonly string[] _allowedOrigins;

    public SmartLinksController(
        IMediator mediator, 
        IClientContextService clientContextService,
        IOptions<FrontendOptions> frontendOptions,
        IOptions<CorsSettingsOptions> corsOptions)
    {
        _mediator = mediator;
        _clientContextService = clientContextService;
        _defaultFrontendUrl = frontendOptions.Value.BaseUrl.TrimEnd('/');
        _allowedOrigins = corsOptions.Value.AllowedOrigins ?? Array.Empty<string>();
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> ResolveAndRedirect(string code, CancellationToken ct)
    {
        var clientContext = _clientContextService.GetClientContext();
        string? referrer = Request.Headers.Referer.ToString();

        string targetBaseUrl = _defaultFrontendUrl; 

        if (!string.IsNullOrWhiteSpace(referrer) && Uri.TryCreate(referrer, UriKind.Absolute, out var refererUri))
        {
            string origin = $"{refererUri.Scheme}://{refererUri.Authority}"; 

            if (_allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                targetBaseUrl = origin; 
            }
        }

        var query = new ResolveSmartLinkQuery(
            Code: code,
            CountryCode: null, 
            Referrer: string.IsNullOrWhiteSpace(referrer) ? null : referrer,
            DeviceType: clientContext.Device 
        );

        var result = await _mediator.Send(query, ct);

        return Redirect($"{targetBaseUrl}{result.RelativePath}");
    }
}