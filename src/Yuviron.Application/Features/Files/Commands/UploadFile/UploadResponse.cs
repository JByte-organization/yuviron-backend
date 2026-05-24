namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed record UploadResponse(
    Guid FileId,
    string Url
);