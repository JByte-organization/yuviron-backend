using Yuviron.Application.Abstractions.Data;
namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IAuditingContext : IDataContext
{
    System.Linq.IQueryable<Yuviron.Domain.Entities.CopyrightClaim> CopyrightClaims { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.VerificationRequest> VerificationRequests { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Complaint> Complaints { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ComplaintCounter> ComplaintCounters { get; }
}