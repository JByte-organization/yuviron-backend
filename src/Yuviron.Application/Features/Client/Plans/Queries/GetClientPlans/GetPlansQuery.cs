using MediatR;
using System.Collections.Generic;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Plans.Queries.GetClientPlans;

public sealed record GetPlansQuery(PlanType? Type = null) : IRequest<List<PlanDto>>;