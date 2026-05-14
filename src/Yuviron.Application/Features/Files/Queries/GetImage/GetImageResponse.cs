namespace Yuviron.Application.Features.Files.Queries.GetImage;

public sealed record GetImageResponse(Stream Stream, string ContentType);