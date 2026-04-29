using MediatR;
using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Client.Home.Queries.GetHomeBanners;


public record GetHomeBannersQuery(int Limit = 5) : IRequest<List<HomeBannerDto>>;