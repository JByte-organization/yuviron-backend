using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Files.Commands.DeleteFile;
using Yuviron.Application.Features.Files.Commands.UploadFile;

namespace Yuviron.Api.Controllers;

[Authorize] 
[Route("api/files")]
public class FilesController : ApiControllerBase
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadResponse>> UploadFile(IFormFile file, [FromForm] string folder, CancellationToken cancellationToken)
    {
        var command = new UploadFileCommand(
            file?.OpenReadStream() ?? Stream.Null,
            file?.FileName ?? string.Empty,
            file?.ContentType ?? string.Empty,
            folder ?? string.Empty
        );

        return Ok(await Mediator.Send(command, cancellationToken));
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFile([FromBody] DeleteFileCommand command, CancellationToken cancellationToken)
    {
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }
    
}