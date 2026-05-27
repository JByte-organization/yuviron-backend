namespace Yuviron.Application.Features.Files.Queries.GetImage;

public sealed record GetPublicFileResponse(Stream Stream, string ContentType);