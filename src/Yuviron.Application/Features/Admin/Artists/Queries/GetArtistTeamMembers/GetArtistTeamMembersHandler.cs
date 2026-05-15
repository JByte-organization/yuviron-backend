using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistTeamMembers;

public sealed class GetArtistTeamMembersHandler : IRequestHandler<GetArtistTeamMembersQuery, PaginatedList<ArtistTeamMemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtistTeamMembersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ArtistTeamMemberDto>> Handle(GetArtistTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == request.ArtistId
                         && !tm.Artist.IsDeleted
                         && !tm.User.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(tm => 
                tm.User.Email.Contains(request.SearchTerm) || 
                tm.User.Profile.FirstName.Contains(request.SearchTerm));
        }

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder, 
            defaultSortBy: nameof(ArtistTeamMember.CreatedAt), 
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<ArtistTeamMember, object>>>
            {
                ["Email"] = tm => tm.User.Email,
                ["FirstName"] = tm => tm.User.Profile.FirstName,
                ["JoinedAt"] = tm => tm.CreatedAt 
            });

        var projectedQuery = sortedQuery.Select(tm => new ArtistTeamMemberDto(
            tm.UserId,
            tm.User.Email,
            tm.User.Profile.FirstName,
            tm.User.Profile.AvatarUrl,
            tm.User.AccountState,
            tm.Role,
            tm.CreatedAt      
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
