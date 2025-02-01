namespace SilkyUIFramework;

public struct Size(float width, float height)
{
    public float Width { get; set; } = width;
    public float Height { get; set; } = height;

    public static implicit operator Size(Vector2 vector2)
    {
        return new Size(vector2.X, vector2.Y);
    }

    public static implicit operator Vector2(Size size)
    {
        return new Vector2(size.Width, size.Height);
    }
}
