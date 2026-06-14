using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Queries.GetRequests;

public sealed class GetVerificationRequestsHandler : IRequestHandler<GetVerificationRequestsQuery, PaginatedList<VerificationRequestListItemDto>>
{
    private readonly IAuditingContext _auditingContext;

    public GetVerificationRequestsHandler(IAuditingContext auditingContext)
    {
        _auditingContext = auditingContext;
    }

    public async Task<PaginatedList<VerificationRequestListItemDto>> Handle(GetVerificationRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _auditingContext.VerificationRequests
            .AsNoTracking()
            .Include(vr => vr.Artist)
            .Include(vr => vr.SubmittedByUser)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(vr => vr.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(vr => vr.Artist.Name.Contains(request.SearchTerm) || vr.SubmittedByUser.Email.Contains(request.SearchTerm));

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(VerificationRequest.CreatedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<VerificationRequest, object>>>
            {
                ["ArtistName"] = vr => vr.Artist.Name,
                ["UserEmail"] = vr => vr.SubmittedByUser.Email
            });

        var projectedQuery = sortedQuery.Select(vr => new VerificationRequestListItemDto(
            vr.Id,
            vr.ArtistId,
            vr.Artist.Name,
            vr.SubmittedByUserId,
            vr.SubmittedByUser.Email,
            vr.ClaimedRole,
            vr.Status,
            vr.CreatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}