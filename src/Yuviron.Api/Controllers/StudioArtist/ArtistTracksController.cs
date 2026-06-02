using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Yuviron.Application.Features.Files.Commands.UploadFile;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;

namespace Yuviron.Api.Controllers.StudioArtist;

[Authorize]
[Route("api/studio-artist/tracks")]
[ApiExplorerSettings(GroupName = "artist")]
public class ArtistTracksController : ApiControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CreateStudioArtistTrackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateStudioArtistTrackResponse>> CreateTrack(
        [FromForm] CreateStudioArtistTrackRequest request,
        CancellationToken ct)
    {
        Stream audioStream = request.AudioFile?.OpenReadStream() ?? Stream.Null;
        using (audioStream)
        {
            var audioUpload = await Mediator.Send(new UploadFileCommand(
                audioStream,
                request.AudioFile?.FileName ?? string.Empty,
                request.AudioFile?.ContentType ?? string.Empty), ct);

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

            var trackId = await Mediator.Send(new CreateStudioArtistTrackCommand(
                request.AlbumId,
                request.Title,
                request.Explicit,
                audioUpload.FileId,
                coverFileId,
                request.CoAuthorIds), ct);

            return Ok(new CreateStudioArtistTrackResponse(trackId));
        }
    }

    public sealed record CreateStudioArtistTrackRequest(
        Guid AlbumId,
        string Title,
        bool Explicit,
        IFormFile? AudioFile,
        IFormFile? CoverFile,
        List<Guid>? CoAuthorIds
    );

    public sealed record CreateStudioArtistTrackResponse(Guid TrackId);
}
