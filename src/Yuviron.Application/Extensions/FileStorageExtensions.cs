using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Extensions;

public static class FileStorageExtensions
{
    // Этот метод оставляем как есть — он физически двигает файл
    public static async Task<string?> MoveIfTempAsync(
        this IFileStorageService storage,
        string? path,
        string destinationFolder,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("temp/"))
        {
            return path;
        }

        return await storage.MoveAsync(path!, destinationFolder, ct);
    }

    // НОВЫЙ МЕТОД: Он просто генерирует строку с новым путем, 
    // чтобы мы могли сохранить ее в базу ДО перемещения файла
    public static string? PredictDestinationPath(string? path, string destinationFolder)
    {
        // Если файла нет или он уже лежит где надо (не в temp) — возвращаем как есть
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("temp/"))
        {
            return path;
        }

        // Вытаскиваем имя файла (например, "123.jpg") и клеим к новой папке
        var fileName = Path.GetFileName(path);
        return Path.Combine(destinationFolder, fileName).Replace("\\", "/");
    }
}