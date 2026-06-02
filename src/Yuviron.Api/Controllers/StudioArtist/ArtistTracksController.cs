using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Files.Commands.UploadFile;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetTracks;

namespace Yuviron.Api.Controllers.StudioArtist;

[Authorize]
[Route("api/studio-artist/tracks")]
[ApiExplorerSettings(GroupName = "artist")]
public class ArtistTracksController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<StudioArtistTrackListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<StudioArtistTrackListItemDto>>> GetTracks(
        [FromQuery] GetStudioArtistTracksQuery query,
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

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

    public sealed record CreateStudioArtistTrackRequest(
        Guid AlbumId,
        string Title,
        bool Explicit,
        IFormFile? AudioFile,
        IFormFile? CoverFile,
        List<Guid>? CoAuthorIds
    );

    public sealed record CreateStudioArtistTrackResponse(Guid TrackId);

    public sealed record UpdateStudioArtistTrackRequest(
        string Title,
        IFormFile? CoverFile,
        List<Guid>? CoAuthorIds
    );
}