namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed record UploadResponse(
    string Path, 
    string Url
);