using MediatR;
using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Client.Genres.Queries.GetGenres;


public record GetGenresQuery(int Limit = 20) : IRequest<List<GenreItemDto>>;