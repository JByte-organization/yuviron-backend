using MediatR;

namespace Yuviron.Application.Features.Admin.Ads.Commands.ToggleAdStatus;

public sealed record ToggleAdStatusCommand(Guid AdId, bool IsActive) : IRequest;