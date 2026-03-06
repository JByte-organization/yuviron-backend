using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.Services;

public interface IFileStorageService
{
    // Добавили параметр folder (например: "avatars", "tracks")
    Task<string> UploadAsync(Stream stream, string folder, string fileName, string contentType, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);
}