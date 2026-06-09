using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Complaints.Queries.GetComplaintReasons;

public sealed record ComplaintReasonDto(
    ComplaintReasonCode Code,
    string Label);
