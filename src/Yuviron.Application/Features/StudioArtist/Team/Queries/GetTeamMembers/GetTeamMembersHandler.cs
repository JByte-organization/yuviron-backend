using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Team.Queries.GetTeamMembers;

public sealed class GetTeamMembersHandler : IRequestHandler<GetTeamMembersQuery, List<TeamMemberDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTeamMembersHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context; _currentUser = currentUser;
    }

    public async Task<List<TeamMemberDto>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _context.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId, cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this artist's team.");

        var members = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Include(tm => tm.User)
            .ThenInclude(u => u!.Profile)
            .Where(tm => tm.ArtistId == request.ArtistId)
            .Select(tm => new TeamMemberDto(
                tm.UserId,
                tm.User.Email,
                tm.User.Profile != null ? tm.User.Profile.FirstName : string.Empty,
                tm.User.Profile != null ? tm.User.Profile.AvatarUrl : null,
                tm.Role,
                tm.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return members;
    }
}