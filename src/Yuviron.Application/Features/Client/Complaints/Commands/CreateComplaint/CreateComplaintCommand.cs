using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Complaints.Commands.CreateComplaint;

public sealed record CreateComplaintCommand(
    ComplaintTargetType TargetType,
    Guid TargetId,
    ComplaintReasonCode ReasonCode,
    string? Comment
) : IRequest<Guid>;
