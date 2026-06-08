using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Complaints.Commands.CreateComplaint;

public sealed record CreateComplaintCommand(
    ComplaintTargetType TargetType,
    Guid TargetId,
    string ReasonCode,
    string? Comment
) : IRequest<Guid>;
