
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IAuditingContext : IDataContext
{
    IQueryable<CopyrightClaim> CopyrightClaims { get; }
    IQueryable<VerificationRequest> VerificationRequests { get; }
    IQueryable<Complaint> Complaints { get; }
    IQueryable<ComplaintCounter> ComplaintCounters { get; }
}