using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Yuviron.Application.Features.Files.Commands.DeleteFile;
using Yuviron.Application.Features.Files.Commands.UploadFile;

namespace Yuviron.Api.Controllers;

[AllowAnonymous]
[Route("api/files")]
public class FilesController : ApiControllerBase
{
    private readonly string _storageRoot;

    public FilesController(IConfiguration configuration)
    {
        _storageRoot = configuration["FILE_STORAGE_ROOT"] ?? "/var/yuviron/storage";
    }

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

    [HttpGet("download")]
    public IActionResult DownloadTestFile([FromQuery] string folder, [FromQuery] string fileName)
    {
        if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest("Specify the folder and file name.");
        }

        if (fileName.Contains("..") || folder.Contains(".."))
        {
            return BadRequest("Invalid characters in the path.");
        }

        var fullPath = Path.Combine(_storageRoot, folder, fileName);

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound($"The file was not found on the server at the path: {fullPath}");
        }

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fullPath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(fullPath, contentType, fileDownloadName: fileName, enableRangeProcessing: true);
    }
}