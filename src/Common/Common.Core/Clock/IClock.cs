namespace Common.Core.Clock;

public interface IClock
{
    public DateTime UtcNow { get; }
}
