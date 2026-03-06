using MediatR;
using System.IO;

namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed record UploadFileCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    string Folder
) : IRequest<UploadResponse>;