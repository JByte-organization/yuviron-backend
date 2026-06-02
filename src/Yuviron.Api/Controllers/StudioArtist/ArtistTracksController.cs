using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Yuviron.Application.Features.Files.Commands.UploadFile;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;

namespace Yuviron.Api.Controllers.StudioArtist;

[Authorize]
[Route("api/studio-artist/tracks")]
[ApiExplorerSettings(GroupName = "artist")]
public class ArtistTracksController : ApiControllerBase
{
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTrack(
        [FromRoute] Guid id,
        [FromForm] UpdateStudioArtistTrackRequest request,
        CancellationToken ct)
    {
        Guid? coverFileId = null;

        if (request.CoverFile is not null)
        {
            Stream coverStream = request.CoverFile.OpenReadStream();
            using (coverStream)
            {
                var coverUpload = await Mediator.Send(new UploadFileCommand(
                    coverStream,
                    request.CoverFile.FileName,
                    request.CoverFile.ContentType), ct);

                coverFileId = coverUpload.FileId;
            }
        }

        await Mediator.Send(new UpdateStudioArtistTrackCommand(
            id,
            request.Title,
            coverFileId,
            request.CoAuthorIds), ct);

        return NoContent();
    }

    public sealed record UpdateStudioArtistTrackRequest(
        string Title,
        IFormFile? CoverFile,
        List<Guid>? CoAuthorIds
    );
}
