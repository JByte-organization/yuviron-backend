using MediatR;
using System;

namespace Yuviron.Application.Features.Client.Ads.Commands.RegisterImpression;

public sealed record RegisterAdImpressionCommand(Guid AdId, string? Context) : IRequest;