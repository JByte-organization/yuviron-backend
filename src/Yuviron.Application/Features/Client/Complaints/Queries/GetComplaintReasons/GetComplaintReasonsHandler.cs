using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Complaints.Queries.GetComplaintReasons;

public sealed class GetComplaintReasonsHandler : IRequestHandler<GetComplaintReasonsQuery, List<ComplaintReasonDto>>
{
    private static readonly ComplaintReasonDto[] Reasons =
    [
        new(ComplaintReasonCode.Spam, "Spam or misleading content"),
        new(ComplaintReasonCode.CopyrightViolation, "Copyright violation"),
        new(ComplaintReasonCode.Pornography, "Pornography or explicit content"),
        new(ComplaintReasonCode.HateSpeech, "Hate speech"),
        new(ComplaintReasonCode.Abuse, "Harassment or abuse"),
        new(ComplaintReasonCode.Scam, "Scam or fraud"),
        new(ComplaintReasonCode.Other, "Other")
    ];

    public Task<List<ComplaintReasonDto>> Handle(GetComplaintReasonsQuery request, CancellationToken cancellationToken)
        => Task.FromResult(Reasons.ToList());
}
