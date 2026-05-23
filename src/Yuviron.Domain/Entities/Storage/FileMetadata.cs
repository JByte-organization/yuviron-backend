using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class FileMetadata : Entity
{
    public Guid UserId { get; private set; } 
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty; 
    public long SizeBytes { get; private set; }
    
    public bool IsTemporary { get; private set; } 
    public string CurrentStorageKey { get; private set; } = string.Empty; 
    
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;

    private FileMetadata() { }

    public static FileMetadata Create(Guid id, Guid userId, string originalName, string contentType, long sizeBytes, string storageKey, DateTime utcNow)
    {
        return new FileMetadata
        {
            Id = id,
            UserId = userId,
            OriginalFileName = originalName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            IsTemporary = true,
            CurrentStorageKey = storageKey,
            CreatedAt = utcNow
        };
    }

    public void MarkAsPermanent(string newStorageKey)
    {
        IsTemporary = false;
        CurrentStorageKey = newStorageKey;
    }
}