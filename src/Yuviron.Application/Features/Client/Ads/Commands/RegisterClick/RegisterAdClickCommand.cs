using MediatR;

namespace Yuviron.Application.Features.Client.Ads.Commands.RegisterClick;

public sealed record RegisterAdClickCommand(Guid AdId) : IRequest;