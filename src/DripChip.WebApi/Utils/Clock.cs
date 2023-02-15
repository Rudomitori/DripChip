using DripChip.Domain.Utils;

namespace DripChip.WebApi.Utils;

// Source: https://stackoverflow.com/a/153014
public sealed class Clock : IClock
{
    private readonly long _roundTicks;

    public Clock(long roundTicks) => _roundTicks = roundTicks;

    public DateTime UtcNow
    {
        get
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Ticks - now.Ticks % _roundTicks, now.Kind);
        }
    }
}
