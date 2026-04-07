using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistTeamMembers;

public sealed class GetArtistTeamMembersHandler : IRequestHandler<GetArtistTeamMembersQuery, List<ArtistTeamMemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtistTeamMembersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArtistTeamMemberDto>> Handle(GetArtistTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var teamMembers = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == request.ArtistId)
            .OrderByDescending(tm => tm.Role == ArtistTeamRole.Owner)
            .ThenBy(tm => tm.CreatedAt)
            .Select(tm => new ArtistTeamMemberDto(
                tm.UserId,
                tm.User.Email,
                tm.User.Profile.FirstName,
                tm.User.Profile.AvatarUrl,
                tm.User.AccountState,
                tm.Role,
                tm.CreatedAt      
            ))
            .ToListAsync(cancellationToken);

        return teamMembers;
    }
}