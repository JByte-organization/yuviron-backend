using MediatR;

namespace Yuviron.Application.Features.Client.Complaints.Queries.GetComplaintReasons;

public sealed record GetComplaintReasonsQuery : IRequest<List<ComplaintReasonDto>>;
