using MediatR;

namespace Yuviron.Application.Features.Files.Commands.DeleteFile;

public sealed record DeleteFileCommand(string FilePath) : IRequest<Unit>;