using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Artists.Commands.AddTeamMember;
using Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;
using Yuviron.Application.Features.Admin.Artists.Commands.DeleteArtist;
using Yuviron.Application.Features.Admin.Artists.Commands.RemoveTeamMember;
using Yuviron.Application.Features.Admin.Artists.Commands.UpdateArtist;
using Yuviron.Application.Features.Admin.Artists.Commands.UpdateTeamMemberRole;
using Yuviron.Application.Features.Admin.Artists.Queries;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Application.Features.Admin.Artists.Queries.GetArtistById;
using Yuviron.Application.Features.Admin.Artists.Queries.GetArtists;
using Yuviron.Application.Features.Admin.Artists.Queries.GetArtistsAutocomplete;
using Yuviron.Application.Features.Admin.Artists.Queries.GetArtistTeamMembers;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/artists")]
public class AdminArtistsController : AdminApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ArtistListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<ArtistListItemDto>>> GetArtists([FromQuery] GetArtistsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ArtistDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ArtistDetailsDto>> GetArtistById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetArtistByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateArtistResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateArtistResponse>> CreateArtist([FromBody] CreateArtistCommand command, CancellationToken ct)
    {
        var artistId = await Mediator.Send(command, ct);
        return Ok(new CreateArtistResponse(artistId));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateArtist(Guid id, [FromBody] UpdateArtistCommand command, CancellationToken ct)
    {
        var commandWithId = command with { ArtistId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteArtist(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteArtistCommand(id), ct);
        return NoContent();
    }
    
        
    [HttpGet("{id:guid}/team")]
    [ProducesResponseType(typeof(List<ArtistTeamMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ArtistTeamMemberDto>>> GetArtistTeam(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetArtistTeamMembersQuery(id), ct);
        return Ok(result);
    }
    
    [HttpPost("{id:guid}/team")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddTeamMember(Guid id, [FromBody] AddTeamMemberCommand command, CancellationToken ct)
    {
        var commandWithId = command with { ArtistId = id };
        await Mediator.Send(commandWithId, ct);
        
        return NoContent();
    }

    [HttpPut("{id:guid}/team/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateTeamMemberRole(Guid id, Guid userId, [FromBody] UpdateTeamMemberRoleCommand command, CancellationToken ct)
    {
        var commandWithIds = command with { ArtistId = id, UserId = userId };
        await Mediator.Send(commandWithIds, ct);
        
        return NoContent();
    }

    [HttpDelete("{id:guid}/team/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTeamMember(Guid id, Guid userId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTeamMemberCommand(id, userId), ct);
        
        return NoContent();
    }
    
    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(List<ArtistAutocompleteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ArtistAutocompleteDto>>> Autocomplete([FromQuery] string searchTerm, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var query = new GetArtistsAutocompleteQuery(searchTerm, limit);
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

}
