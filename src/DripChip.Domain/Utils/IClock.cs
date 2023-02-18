namespace DripChip.Domain.Utils;

public interface IClock
{
    public DateTime UtcNow { get; }
}
