using MediatR;
using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Client.Home.Queries.GetNewReleases;



public record GetNewReleasesQuery(int Limit = 10) : IRequest<List<NewReleaseDto>>;