using MediatR;
using System.IO;

namespace Yuviron.Application.Features.Files.Queries.GetImage;

public sealed record GetPublicFileQuery(string Hash) : IRequest<GetPublicFileResponse>;
