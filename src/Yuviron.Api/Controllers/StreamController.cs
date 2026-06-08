using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Stream.Queries.GetAudioStream;

namespace Yuviron.Api.Controllers.Client;

[Route("api/stream")]
[ApiExplorerSettings(GroupName = "client")]
public class StreamController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet("tracks/{trackId:guid}/{quality:int}/{**fileName}")] 
    public async Task<IActionResult> GetAudioStream(
        [FromRoute] Guid trackId, 
        [FromRoute] int quality, 
        [FromRoute] string fileName, 
        [FromQuery] long exp, 
        [FromQuery] Guid uid, 
        [FromQuery] string sig, 
        CancellationToken ct = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var query = new GetAudioStreamQuery(trackId, quality, fileName, exp, uid, sig, ipAddress, userAgent);
        var result = await Mediator.Send(query, ct);

        return File(result.Stream, result.ContentType, enableRangeProcessing: true);
    }
}