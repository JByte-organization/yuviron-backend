using System.IO;

namespace Yuviron.Application.Features.Files.Queries.GetTempPreview;

public sealed record GetTempPreviewResponse(
    Stream Stream, 
    string ContentType
);