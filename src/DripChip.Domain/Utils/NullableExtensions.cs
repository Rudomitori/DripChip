namespace DripChip.Domain.Utils;

public static class NullableExtensions
{
    public static bool TrySet<T>(ref this T target, T? nullable)
        where T : struct
    {
        if (nullable is { } value)
        {
            target = value;
            return true;
        }

        return false;
    }
}
