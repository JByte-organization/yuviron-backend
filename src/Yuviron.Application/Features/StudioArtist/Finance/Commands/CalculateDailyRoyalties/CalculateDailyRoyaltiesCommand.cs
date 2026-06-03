using MediatR;
using System;

namespace Yuviron.Application.Features.Finance.Commands.CalculateDailyRoyalties;

public sealed record CalculateDailyRoyaltiesCommand(DateOnly TargetDate) : IRequest;