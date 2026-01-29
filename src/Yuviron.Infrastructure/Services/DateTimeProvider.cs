using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}