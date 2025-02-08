namespace SilkyUIFramework;

public struct Margin
{
    public float Left { get; set; }
    public float Top { get; set; }
    public float Right { get; set; }
    public float Bottom { get; set; }

    public readonly float Width => Left + Right;
    public readonly float Height => Top + Bottom;

    public Margin(float uniform)
    {
        Left = uniform;
        Top = uniform;
        Right = uniform;
        Bottom = uniform;
    }

    public Margin(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public static Vector2 operator +(Vector2 position, Margin margin)
    {
        return new Vector2(position.X + margin.Left, position.Y + margin.Top);
    }

    public static Vector2 operator -(Vector2 position, Margin margin)
    {
        return new Vector2(position.X - margin.Left, position.Y - margin.Top);
    }

    public static bool operator ==(Margin left, Margin right)
    {
        return left.Left == right.Left && left.Top == right.Top && left.Right == right.Right && left.Bottom == right.Bottom;
    }

    public static bool operator !=(Margin left, Margin right)
    {
        return !(left == right);
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Margin margin && this == margin;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Left, Top, Right, Bottom);
    }
}