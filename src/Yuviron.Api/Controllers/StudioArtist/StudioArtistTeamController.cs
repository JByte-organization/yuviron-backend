using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.StudioArtist.Team.Commands.AddTeamMember;
using Yuviron.Application.Features.StudioArtist.Team.Commands.AcceptTeamInvite; // <--- ДОДАЛИ
using Yuviron.Application.Features.StudioArtist.Team.Commands.RemoveTeamMember;
using Yuviron.Application.Features.StudioArtist.Team.Commands.UpdateTeamMemberRole;
using Yuviron.Application.Features.StudioArtist.Team.Queries.GetTeamMembers;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/team")] 
public class StudioArtistTeamController : StudioArtistApiControllerBase
{
    [HttpGet("{artistId:guid}")]
    [ProducesResponseType(typeof(List<TeamMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TeamMemberDto>>> GetTeamMembers(Guid artistId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTeamMembersQuery(artistId), ct);
        return Ok(result);
    }

    [HttpPost("{artistId:guid}/invite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> InviteTeamMember(Guid artistId, [FromBody] AddTeamMemberCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { ArtistId = artistId }, ct);
        return NoContent();
    }


    [HttpPut("{artistId:guid}/{targetUserId:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRole(Guid artistId, Guid targetUserId, [FromBody] UpdateTeamMemberRoleCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { ArtistId = artistId, TargetUserId = targetUserId }, ct);
        return NoContent();
    }

    [HttpDelete("{artistId:guid}/{targetUserId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(Guid artistId, Guid targetUserId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTeamMemberCommand(artistId, targetUserId), ct);
        return NoContent();
    }
}