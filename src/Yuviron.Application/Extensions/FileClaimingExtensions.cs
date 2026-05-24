using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Common;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Extensions;

public record ClaimedFileResult(string FinalPath, string SourceKey, string DestinationFolder);

public static class FileClaimingExtensions
{
    public static async Task<ClaimedFileResult> ClaimFileAsync(
        this IApplicationDbContext context,
        Guid fileId,
        Guid currentUserId,
        string expectedContentTypePrefix,
        string destinationFolder, 
        CancellationToken cancellationToken)
    {
        var fileMeta = await context.FileMetadata.FindAsync(new object[] { fileId }, cancellationToken);

        if (fileMeta == null) throw new NotFoundException("File", fileId);
        if (fileMeta.UserId != currentUserId) throw new UnauthorizedAccessException("You don't have access to this file.");
        
        if (!fileMeta.ContentType.StartsWith(expectedContentTypePrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Invalid file format. Expected {expectedContentTypePrefix}.");

        if (!fileMeta.IsTemporary)
        {
            return new ClaimedFileResult(fileMeta.CurrentStorageKey, fileMeta.CurrentStorageKey, destinationFolder);
        }

        var sourceStorageKey = fileMeta.CurrentStorageKey;
        var finalPath = $"{destinationFolder}/{fileMeta.Id:N}";

        fileMeta.MarkAsPermanent(finalPath);

        return new ClaimedFileResult(finalPath, sourceStorageKey, destinationFolder);
    }
    
    public static void RegisterFileSwapEvents(
        this Entity entity, 
        ClaimedFileResult newClaim, 
        string? oldFileUrl = null)
    {
        if (newClaim.SourceKey.StartsWith("temp/"))
        {
            entity.AddDomainEvent(new TempFileNeedsMovingEvent(newClaim.SourceKey, newClaim.DestinationFolder));
        }

        if (!string.IsNullOrWhiteSpace(oldFileUrl))
        {
            entity.AddDomainEvent(new FileNeedsDeletionEvent(oldFileUrl));
        }
    }
}