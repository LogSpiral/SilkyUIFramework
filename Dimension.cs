using System.Diagnostics.CodeAnalysis;

namespace SilkyUIFramework;

public readonly struct Dimension(float pixels = 0f, float percent = 0f) : IEquatable<Dimension>, IParsable<Dimension>
{
    public float Pixels { get; } = pixels;
    public float Percent { get; } = percent;

    public float CalculateSize(float containerSize) => Pixels + containerSize * Percent;

    public Dimension With(float? pixels = null, float? percent = null) =>
        new(pixels ?? Pixels, percent ?? Percent);

    public static bool operator ==(Dimension left, Dimension right) => left.Equals(right);
    public static bool operator !=(Dimension left, Dimension right) => !left.Equals(right);

    public bool Equals(Dimension other) => Pixels.Equals(other.Pixels) && Percent.Equals(other.Percent);
    public override bool Equals(object obj) => obj is Dimension other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Pixels, Percent);

    public override string ToString() => $"{Pixels}px {Percent}%";

    public static Dimension Parse(string s, IFormatProvider provider)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("Input string cannot be null or empty.", nameof(s));

        var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        switch (parts.Length)
        {
            case 0: goto default;
            case 1:
            {
                if (parts[0].EndsWith("px") && float.TryParse(parts[0].TrimEnd("px"), out var pixels))
                {
                    return new Dimension(pixels, 0f);
                }
                else if (parts[0].EndsWith('%') && float.TryParse(parts[0].TrimEnd("%"), out var percent))
                {
                    return new Dimension(0f, percent);
                }

                goto default;
            }
            case 2:
            {
                if (!float.TryParse(parts[0].TrimEnd("px"), out var pixels)) goto default;
                if (!float.TryParse(parts[1].TrimEnd("%"), out var percent)) goto default;
                return new Dimension(pixels, percent / 100f);
            }
            default: throw new FormatException($"Cannot parse '{s}' as Dimension.");
        }

    }

    public static bool TryParse([NotNullWhen(true)] string s, IFormatProvider provider, [MaybeNullWhen(false)] out Dimension result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = new Dimension();
            return false;
        }

        var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        switch (parts.Length)
        {
            case 0: goto default;
            case 1:
            {
                if (parts[0].EndsWith("px") && float.TryParse(parts[0].TrimEnd("px"), out var pixels))
                {
                    result = new Dimension(pixels, 0f);
                    return true;
                }
                else if (parts[0].EndsWith('%') && float.TryParse(parts[0].TrimEnd("%"), out var percent))
                {
                    result = new Dimension(0f, percent);
                    return true;
                }

                goto default;
            }
            case 2:
            {
                if (!float.TryParse(parts[0].TrimEnd("px"), out var pixels)) goto default;
                if (!float.TryParse(parts[1].TrimEnd("%"), out var percent)) goto default;
                result = new Dimension(pixels, percent / 100f);
                return true;
            }
            default:
            {
                result = new Dimension();
                return false;
            }
        }
    }
}