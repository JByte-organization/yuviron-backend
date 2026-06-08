using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Complaints.Queries.GetComplaintById;

public sealed class GetComplaintByIdHandler : IRequestHandler<GetComplaintByIdQuery, ComplaintDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetComplaintByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ComplaintDetailsDto> Handle(GetComplaintByIdQuery request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .AsNoTracking()
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (complaint == null) throw new NotFoundException(nameof(Complaint), request.ComplaintId);

        var counter = await _context.ComplaintCounters
            .AsNoTracking()
            .FirstOrDefaultAsync(cc => cc.TargetType == complaint.TargetType && cc.TargetId == complaint.TargetId, cancellationToken);

        return new ComplaintDetailsDto(
            complaint.Id,
            complaint.CreatedByUserId,
            complaint.CreatedByUser.Email,
            complaint.TargetType,
            complaint.TargetId,
            complaint.ReasonCode,
            complaint.Comment,
            complaint.Status,
            complaint.ModeratedByAdminId,
            complaint.ModerationNote,
            complaint.CreatedAt,
            complaint.UpdatedAt,
            counter?.CountTotal ?? 1
        );
    }
}