using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Complaints.Queries.GetComplaints;

public sealed class GetComplaintsHandler : IRequestHandler<GetComplaintsQuery, PaginatedList<ComplaintListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ComplaintListItemDto>> Handle(GetComplaintsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Complaints
            .AsNoTracking()
            .Include(c => c.CreatedByUser)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (request.TargetType.HasValue)
            query = query.Where(c => c.TargetType == request.TargetType.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(c => c.ReasonCode.Contains(request.SearchTerm) || c.CreatedByUser.Email.Contains(request.SearchTerm));

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Complaint.CreatedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Complaint, object>>>
            {
                ["UserEmail"] = c => c.CreatedByUser.Email
            });

        var projectedQuery = sortedQuery.Select(c => new ComplaintListItemDto(
            c.Id,
            c.TargetType,
            c.TargetId,
            c.ReasonCode,
            c.Status,
            c.CreatedAt,
            c.CreatedByUserId,
            c.CreatedByUser.Email
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}