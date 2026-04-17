using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Infrastructure.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
