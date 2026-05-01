using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Files.Commands.UploadFile;
using Yuviron.Application.Features.Files.Queries.GetTempPreview;

namespace Yuviron.Api.Controllers;

[Authorize] 
[Route("api/files")]
public class FilesController : ApiControllerBase
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadResponse>> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        var command = new UploadFileCommand(
            file?.OpenReadStream() ?? Stream.Null,
            file?.FileName ?? string.Empty,
            file?.ContentType ?? string.Empty
        );

        return Ok(await Mediator.Send(command, cancellationToken));
    }

    [HttpGet("temp/{fileName}")]
    [AllowAnonymous] 
    public async Task<IActionResult> GetTempPreview(string fileName, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTempPreviewQuery(fileName), ct);
        
        return File(result.Stream, result.ContentType, enableRangeProcessing: true);
    }
}