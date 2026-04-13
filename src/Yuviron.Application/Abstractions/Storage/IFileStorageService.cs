using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream stream, string folder, string fileName, string contentType, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);
    
    Task<string> MoveAsync(string sourceFileKey, string destinationFolder, CancellationToken cancellationToken = default);
    
    Task<Stream> GetFileStreamAsync(string fileKey, CancellationToken cancellationToken);
    
    Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);
}