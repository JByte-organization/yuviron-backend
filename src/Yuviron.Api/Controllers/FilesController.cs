using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            return BadRequest("Укажите папку и имя файла.");
        }

        if (fileName.Contains("..") || folder.Contains(".."))
        {
            return BadRequest("Недопустимые символы в пути.");
        }

        var fullPath = Path.Combine(_storageRoot, folder, fileName);

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound($"Файл не найден на сервере по пути: {fullPath}");
        }

        return PhysicalFile(fullPath, "application/octet-stream", enableRangeProcessing: true);
    }
}