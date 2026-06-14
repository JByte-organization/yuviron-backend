using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Queries.GetRequestById;

public sealed class GetVerificationRequestByIdHandler : IRequestHandler<GetVerificationRequestByIdQuery, VerificationRequestDetailDto>
{
    private readonly IAuditingContext _auditingContext;

    public GetVerificationRequestByIdHandler(IAuditingContext auditingContext)
    {
        _auditingContext = auditingContext;
    }

    public async Task<VerificationRequestDetailDto> Handle(GetVerificationRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var vr = await _auditingContext.VerificationRequests
            .AsNoTracking()
            .Include(vr => vr.Artist)
            .Include(vr => vr.SubmittedByUser)
            .FirstOrDefaultAsync(vr => vr.Id == request.RequestId, cancellationToken);

        if (vr == null)
            throw new NotFoundException(nameof(VerificationRequest), request.RequestId);

        return new VerificationRequestDetailDto(
            vr.Id, 
            vr.ArtistId, 
            vr.Artist.Name, 
            vr.Artist.AvatarUrl,
            vr.SubmittedByUserId, 
            vr.SubmittedByUser.Email,
            vr.ClaimedRole, 
            vr.OfficialEmail, 
            vr.Links, 
            vr.ProofFileUrl, 
            vr.Message,
            vr.Status, 
            vr.AdminNote, 
            vr.CreatedAt, 
            vr.UpdatedAt
        );
    }
}