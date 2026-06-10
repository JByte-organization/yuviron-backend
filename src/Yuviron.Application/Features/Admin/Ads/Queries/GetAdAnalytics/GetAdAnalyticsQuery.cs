using System;
using System.Collections.Generic;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Ads.Queries.GetAdAnalytics;

public sealed record GetAdAnalyticsQuery(Guid AdId, string Interval, DateTime MinDate) : IRequest<List<AdAnalyticsPointDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}
