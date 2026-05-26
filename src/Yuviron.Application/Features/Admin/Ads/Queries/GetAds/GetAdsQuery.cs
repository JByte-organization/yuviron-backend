using MediatR;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Admin.Ads.Queries.GetAds;

public record GetAdsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<AdSummaryDto>>;