using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Artists.Commands.AddTeamMember;
using Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;
using Yuviron.Application.Features.Admin.Artists.Commands.DeleteArtist;
using Yuviron.Application.Features.Admin.Artists.Commands.RemoveTeamMember;
using Yuviron.Application.Features.Admin.Artists.Commands.UpdateArtist;
using Yuviron.Application.Features.Admin.Artists.Commands.UpdateTeamMemberRole;
using Yuviron.Application.Features.Admin.Artists.Queries.GetArtistById;
using Yuviron.Application.Features.Admin.Artists.Queries.GetArtists;
using Yuviron.Application.Features.Admin.Artists.Queries.GetArtistTeamMembers;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/artists")]
public class AdminArtistsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetArtists([FromQuery] GetArtistsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetArtistById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetArtistByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateArtist([FromBody] CreateArtistCommand command, CancellationToken ct)
    {
        var artistId = await Mediator.Send(command, ct);
        return Ok(new { ArtistId = artistId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateArtist(Guid id, [FromBody] UpdateArtistCommand command, CancellationToken ct)
    {
        var commandWithId = command with { ArtistId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteArtist(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteArtistCommand(id), ct);
        return NoContent();
    }
    
        
    [HttpGet("{id:guid}/team")]
    public async Task<IActionResult> GetArtistTeam(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetArtistTeamMembersQuery(id), ct);
        return Ok(result);
    }
    
    [HttpPost("{id:guid}/team")]
    public async Task<IActionResult> AddTeamMember(Guid id, [FromBody] AddTeamMemberCommand command, CancellationToken ct)
    {
        var commandWithId = command with { ArtistId = id };
        await Mediator.Send(commandWithId, ct);
        
        return NoContent();
    }

    [HttpPut("{id:guid}/team/{userId:guid}")]
    public async Task<IActionResult> UpdateTeamMemberRole(Guid id, Guid userId, [FromBody] UpdateTeamMemberRoleCommand command, CancellationToken ct)
    {
        var commandWithIds = command with { ArtistId = id, UserId = userId };
        await Mediator.Send(commandWithIds, ct);
        
        return NoContent();
    }

    [HttpDelete("{id:guid}/team/{userId:guid}")]
    public async Task<IActionResult> RemoveTeamMember(Guid id, Guid userId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTeamMemberCommand(id, userId), ct);
        
        return NoContent();
    }

}