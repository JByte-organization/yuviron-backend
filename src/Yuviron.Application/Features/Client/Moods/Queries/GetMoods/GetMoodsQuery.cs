using MediatR;
using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Client.Moods.Queries.GetMoods;



public record GetMoodsQuery(int Limit = 20) : IRequest<List<MoodItemDto>>;