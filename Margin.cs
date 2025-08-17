using System.Diagnostics.CodeAnalysis;

namespace SilkyUIFramework;

public readonly struct Margin(float left, float top, float right, float bottom) : IEquatable<Margin>, IParsable<Margin>
{
    public static Margin Zero { get; } = new(0f, 0f, 0f, 0f);

    public Margin(float uniform) : this(uniform, uniform, uniform, uniform) { }

    public Margin(float horizontal, float vertical) : this(horizontal, vertical, horizontal, vertical) { }

    public void Deconstruct(out float left, out float top, out float right, out float bottom)
    {
        left = Left; top = Top; right = Right; bottom = Bottom;
    }

    public float Left { get; } = left;
    public float Top { get; } = top;
    public float Right { get; } = right;
    public float Bottom { get; } = bottom;

    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;

    public static implicit operator Margin(float margin) => new(margin);

    public Margin With(float? left = null, float? top = null, float? right = null, float? bottom = null) =>
        new(left ?? Left, top ?? Top, right ?? Right, bottom ?? Bottom);

    public static Vector2 operator +(Vector2 position, Margin margin) =>
        new(position.X + margin.Left, position.Y + margin.Top);

    public static Vector2 operator -(Vector2 position, Margin margin) =>
        new(position.X - margin.Left, position.Y - margin.Top);

    public static bool operator ==(Margin left, Margin right) => left.Equals(right);

    public static bool operator !=(Margin left, Margin right) => !left.Equals(right);

    public override bool Equals(object obj) => obj is Margin margin && Equals(margin);

    public bool Equals(Margin other) =>
        Left.Equals(other.Left) && Top.Equals(other.Top) && Right.Equals(other.Right) && Bottom.Equals(other.Bottom);

    public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);

    public override string ToString()
    {
        if (Left == Top && Top == Right && Right == Bottom)
            return $"Margin({Left})";
        if (Left == Right && Top == Bottom)
            return $"Margin({Left}, {Top})";
        return $"Margin({Left}, {Top}, {Right}, {Bottom})";
    }

    public static Margin Parse(string s, IFormatProvider provider)
    {
        if (string.IsNullOrEmpty(s))
            throw new ArgumentNullException(nameof(s), "Size string cannot be null or empty.");

        var parts = s.Split(' ');
        switch (parts.Length)
        {
            case 1:
                // Single value, assume square size
                if (!float.TryParse(parts[0], out var size))
                    throw new FormatException("Size must be a valid floating-point number.");
                return new Margin(size);
            case 2:
                if (!float.TryParse(parts[0], out var h) || !float.TryParse(parts[1], out var v))
                    throw new FormatException("Size must be a valid floating-point number.");
                return new Margin(h, v);
            case 4:
                if (!float.TryParse(parts[0], out var l) ||
                    !float.TryParse(parts[1], out var t) ||
                    !float.TryParse(parts[2], out var r) ||
                    !float.TryParse(parts[3], out var b))
                    throw new FormatException("Size must be a valid floating-point number.");
                return new Margin(l, t, r, b);
            default:
                throw new FormatException("Size string must be in the format 'width x height' or 'size'.");
        }
    }

    public static bool TryParse([NotNullWhen(true)] string s, IFormatProvider provider, [MaybeNullWhen(false)] out Margin result)
    {
        result = Zero;
        if (string.IsNullOrEmpty(s))
            return false;

        var parts = s.Split(' ');
        switch (parts.Length)
        {
            case 1:
                // Single value, assume square size
                if (!float.TryParse(parts[0], out var size))
                    return false;
                result = new Margin(size);
                return true;
            case 2:
                if (!float.TryParse(parts[0], out var h) || !float.TryParse(parts[1], out var v))
                    return false;
                result = new Margin(h, v);
                return true;
            case 4:
                if (!float.TryParse(parts[0], out var l) ||
                    !float.TryParse(parts[1], out var t) ||
                    !float.TryParse(parts[2], out var r) ||
                    !float.TryParse(parts[3], out var b))
                    return false;
                result = new Margin(l, t, r, b);
                return true;
            default:
                return false;
        }
    }
}