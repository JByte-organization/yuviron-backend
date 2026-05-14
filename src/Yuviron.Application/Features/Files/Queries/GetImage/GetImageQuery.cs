using MediatR;
using System.IO;

namespace Yuviron.Application.Features.Files.Queries.GetImage;

public sealed record GetImageQuery(string Hash) : IRequest<GetImageResponse>;
