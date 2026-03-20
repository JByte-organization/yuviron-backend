using System.IO;
using MediatR;

namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed record UploadFileCommand(
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<UploadResponse>;