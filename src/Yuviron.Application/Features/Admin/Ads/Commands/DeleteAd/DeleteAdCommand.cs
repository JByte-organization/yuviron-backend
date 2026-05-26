using MediatR;

namespace Yuviron.Application.Features.Admin.Ads.Commands.DeleteAd;

public sealed record DeleteAdCommand(Guid AdId) : IRequest;