using System.Diagnostics.CodeAnalysis;

namespace SilkyUIFramework;

public readonly struct Size(float width, float height) : IEquatable<Size>, IParsable<Size>
{
    public static readonly Size Zero = new(0, 0);

    public Size(float size) : this(size, size) { }

    public float Width { get; } = width;
    public float Height { get; } = height;

    public Size With(float? width = null, float? height = null)
    {
        return new Size(width ?? Width, height ?? Height);
    }

    public static implicit operator Size(float gap)
    {
        return new Size(gap);
    }

    public static implicit operator Size(Vector2 vector2)
    {
        return new Size(vector2.X, vector2.Y);
    }

    public static implicit operator Vector2(Size size)
    {
        return new Vector2(size.Width, size.Height);
    }

    public static Size operator +(Size size1, Size size2)
    {
        return new Size(size1.Width + size2.Width, size1.Height + size2.Height);
    }

    public static Size operator -(Size size1, Size size2)
    {
        return new Size(size1.Width - size2.Width, size1.Height - size2.Height);
    }

    public static Size operator +(Vector2 vector2, Size size)
    {
        return new Size(vector2.X + size.Width, vector2.Y + size.Height);
    }

    public static Size operator -(Vector2 vector2, Size size)
    {
        return new Size(vector2.X - size.Width, vector2.Y - size.Height);
    }

    public static Size operator +(Size size, Vector2 vector2)
    {
        return new Size(size.Width + vector2.X, size.Height + vector2.Y);
    }

    public static Size operator -(Size size, Vector2 vector2)
    {
        return new Size(size.Width - vector2.X, size.Height - vector2.Y);
    }

    public static Size operator +(Size size, Margin margin)
    {
        return new Size(size.Width + margin.Left + margin.Right, size.Height + margin.Top + margin.Bottom);
    }

    public static Size operator -(Size size, Margin margin)
    {
        return new Size(size.Width - margin.Left - margin.Right, size.Height - margin.Top - margin.Bottom);
    }

    public static Size operator *(Size size, float scale)
    {
        return new Size(size.Width * scale, size.Height * scale);
    }

    public static Size operator /(Size size, float scale)
    {
        return new Size(size.Width / scale, size.Height / scale);
    }

    public static Size operator *(Size size, Size scale)
    {
        return new Size(size.Width * scale.Width, size.Height * scale.Height);
    }

    public static Size operator /(Size size, Size scale)
    {
        return new Size(size.Width / scale.Width, size.Height / scale.Height);
    }

    public static bool operator ==(Size size1, Size size2) => size1.Equals(size2);
    public static bool operator !=(Size size1, Size size2) => !size1.Equals(size2);

    public readonly override bool Equals(object obj) => obj is Size size && Equals(size);
    public readonly bool Equals(Size other) => Width.Equals(other.Width) && Height.Equals(other.Height);

    public readonly override int GetHashCode() => HashCode.Combine(Width, Height);

    public static Size Min(Size size1, Size size2)
    {
        return new Size(MathHelper.Min(size1.Width, size2.Width), MathHelper.Min(size1.Height, size2.Height));
    }

    public static Size Max(Size size1, Size size2)
    {
        return new Size(MathHelper.Max(size1.Width, size2.Width), MathHelper.Max(size1.Height, size2.Height));
    }

    public static Size Clamp(Size value, Size min, Size max)
    {
        return new Size(MathHelper.Clamp(value.Width, min.Width, max.Width),
            MathHelper.Clamp(value.Height, min.Height, max.Height));
    }

    public override string ToString() => $"{Width}x{Height}";

    public static Size Parse(string s, IFormatProvider provider)
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
                return new Size(size);
            case 2:
                if (!float.TryParse(parts[0], out var width) || !float.TryParse(parts[1], out var height))
                    throw new FormatException("Size must be a valid floating-point number.");
                return new Size(width, height);
            default:
                throw new FormatException("Size string must be in the format 'width x height' or 'size'.");
        }
    }

    public static bool TryParse([NotNullWhen(true)] string s, IFormatProvider provider, [MaybeNullWhen(false)] out Size result)
    {
        result = Zero;
        if (string.IsNullOrEmpty(s))
        {
            return false;
        }

        var parts = s.Split(' ');
        switch (parts.Length)
        {
            case 1:
                // Single value, assume square size
                if (!float.TryParse(parts[0], out var size))
                    return false;
                result = new Size(size);
                return true;
            case 2:
                if (!float.TryParse(parts[0], out var width) || !float.TryParse(parts[1], out var height))
                    return false;
                result = new Size(width, height);
                return true;
            default:
                result = Zero;
                return false;
        }
    }
}