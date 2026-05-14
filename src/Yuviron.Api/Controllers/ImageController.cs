using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Files.Queries.GetImage;

namespace Yuviron.Api.Controllers.Client;

[AllowAnonymous] 
[Route("i")]    
[ApiExplorerSettings(GroupName = "client")]
public class ImageController : ApiControllerBase
{
    [HttpGet("{hash}")]
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)] 
    public async Task<IActionResult> GetImage(
        [FromRoute] string hash, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(hash) || hash.Contains(".") || hash.Contains("/"))
        {
            return BadRequest("Invalid image hash.");
        }

        var query = new GetImageQuery(hash);
        var result = await Mediator.Send(query, ct);

        return File(result.Stream, result.ContentType, enableRangeProcessing: true);
    }
}