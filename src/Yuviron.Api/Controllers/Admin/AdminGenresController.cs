using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Admin.Genres.Commands.CreateGenre;
using Yuviron.Application.Features.Admin.Genres.Commands.DeleteGenre;
using Yuviron.Application.Features.Admin.Genres.Commands.UpdateGenre;
using Yuviron.Application.Features.Admin.Genres.Queries.GetGenres;
using Yuviron.Application.Features.Admin.Genres.Queries.GetGenresById;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/genres")]
public class AdminGenresController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetGenres([FromQuery] GetGenresQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetGenreById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetGenreByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGenre([FromBody] CreateGenreCommand command, CancellationToken ct)
    {
        var genreId = await Mediator.Send(command, ct);
        return Ok(new { GenreId = genreId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateGenre(Guid id, [FromBody] UpdateGenreCommand command, CancellationToken ct)
    {
        var commandWithId = command with { GenreId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteGenre(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteGenreCommand(id), ct);
        return NoContent();
    }
    
    
}