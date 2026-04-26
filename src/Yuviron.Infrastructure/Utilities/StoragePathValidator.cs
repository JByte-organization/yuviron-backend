namespace Yuviron.Infrastructure.Utilities;

public static class StoragePathValidator
{
    public static string GetValidatedFullPath(string storageRoot, string subPath)
    {
        if (string.IsNullOrWhiteSpace(subPath))
            throw new ArgumentException("Path cannot be empty.", nameof(subPath));

        if (subPath.Contains("..")) 
            throw new ArgumentException("Directory traversal characters ('..') are not allowed.");
            
        var combinedPath = Path.Combine(storageRoot, subPath);
        var fullPath = Path.GetFullPath(combinedPath);
        
        var rootWithSeparator = storageRoot.EndsWith(Path.DirectorySeparatorChar.ToString()) 
            ? storageRoot 
            : storageRoot + Path.DirectorySeparatorChar;

        var fullPathWithSeparator = fullPath.EndsWith(Path.DirectorySeparatorChar.ToString())
            ? fullPath
            : fullPath + Path.DirectorySeparatorChar;

        if (!fullPathWithSeparator.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)) 
        {
            throw new UnauthorizedAccessException("Access denied. Path is outside the storage root.");
        }
        
        return fullPath;
    }
}