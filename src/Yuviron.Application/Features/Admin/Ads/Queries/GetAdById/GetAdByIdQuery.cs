using MediatR;

namespace Yuviron.Application.Features.Admin.Ads.Queries.GetAdById;

public record GetAdByIdQuery(Guid AdId) : IRequest<AdDetailsDto>;