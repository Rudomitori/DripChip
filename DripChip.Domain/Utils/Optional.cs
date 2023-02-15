namespace DripChip.Domain.Utils;

public readonly struct Optional<T>
{
    public readonly bool HasValue;
    private readonly T _value;

    public Optional(T value)
    {
        HasValue = true;
        _value = value;
    }

    public bool TryGet(out T? value)
    {
        if (HasValue)
        {
            value = _value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public static Optional<T> None => new();

    public static implicit operator Optional<T>(T value) => new(value);
}

public static class OptionalExtensions
{
    public static Optional<T> ToOptional<T>(this T value) => new(value);

    public static bool TrySet<T>(ref this T target, Optional<T> optional)
        where T : struct
    {
        if (optional.TryGet(out var value))
        {
            target = value;
            return true;
        }

        return false;
    }

    public static bool TrySet<T>(this Optional<T> optional, ref T target)
    {
        if (optional.TryGet(out var value))
        {
            target = value;
            return true;
        }

        return false;
    }
}
