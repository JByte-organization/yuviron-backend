using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Files.Commands.DeleteFile;

public sealed class DeleteFileHandler : IRequestHandler<DeleteFileCommand, Unit>
{
    private readonly IFileStorageService _fileStorageService;

    public DeleteFileHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        await _fileStorageService.DeleteAsync(request.FilePath, cancellationToken);
        
        return Unit.Value;
    }
}