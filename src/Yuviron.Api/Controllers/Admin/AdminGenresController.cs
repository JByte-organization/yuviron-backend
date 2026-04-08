using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Genres.Commands.CreateGenre;
using Yuviron.Application.Features.Admin.Genres.Commands.DeleteGenre;
using Yuviron.Application.Features.Admin.Genres.Commands.UpdateGenre;
using Yuviron.Application.Features.Admin.Genres.Queries.DTOs;
using Yuviron.Application.Features.Admin.Genres.Queries.GetGenres;
using Yuviron.Application.Features.Admin.Genres.Queries.GetGenresAutocomplete;
using Yuviron.Application.Features.Admin.Genres.Queries.GetGenresById;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/genres")]
public class AdminGenresController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<GenreListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<GenreListItemDto>>> GetGenres([FromQuery] GetGenresQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GenreDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenreDetailsDto>> GetGenreById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetGenreByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateGenreResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateGenreResponse>> CreateGenre([FromBody] CreateGenreCommand command, CancellationToken ct)
    {
        var genreId = await Mediator.Send(command, ct);
        return Ok(new CreateGenreResponse(genreId));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateGenre(Guid id, [FromBody] UpdateGenreCommand command, CancellationToken ct)
    {
        var commandWithId = command with { GenreId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteGenre(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteGenreCommand(id), ct);
        return NoContent();
    }
    
    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(List<GenreAutocompleteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GenreAutocompleteDto>>> AutocompleteGenres([FromQuery] string searchTerm, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetGenresAutocompleteQuery(searchTerm, limit), ct);
        return Ok(result);
    }
    
}
