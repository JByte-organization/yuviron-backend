using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class ComplaintCounter
{
    public ComplaintTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }

    public int CountOpen { get; private set; }
    public int CountTotal { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ComplaintCounter() { }

    public static ComplaintCounter Create(ComplaintTargetType targetType, Guid targetId, DateTime utcNow)
    {
        return new ComplaintCounter
        {
            TargetType = targetType,
            TargetId = targetId,
            CountOpen = 1,
            CountTotal = 1,
            UpdatedAt = utcNow
        };
    }

    public void IncrementNew(DateTime utcNow)
    {
        CountOpen++;
        CountTotal++;
        UpdatedAt = utcNow;
    }

    public void ResolveOpen(DateTime utcNow)
    {
        if (CountOpen > 0) CountOpen--;
        UpdatedAt = utcNow;
    }
}