namespace SilkyUIFramework.Extensions;

public static class CalculatedStyleExtensions
{
    public static bool Intersects(this CalculatedStyle self, CalculatedStyle other)
    {
        return self.X < other.X + other.Width &&
               self.X + self.Width > other.X &&
               self.Y < other.Y + other.Height &&
               self.Y + self.Height > other.Y;
    }

    public static void SetPosition(this CalculatedStyle calculatedStyle, Vector2 position)
    {
        calculatedStyle.X = position.X;
        calculatedStyle.Y = position.Y;
    }

    public static void SetSize(this CalculatedStyle calculatedStyle, Vector2 size)
    {
        calculatedStyle.Width = size.X;
        calculatedStyle.Height = size.Y;
    }

    public static Vector2 Size(this CalculatedStyle calculatedStyle)
    {
        return new Vector2(calculatedStyle.Width, calculatedStyle.Height);
    }

    public static Vector2 LeftBottom(this CalculatedStyle calculatedStyle)
    {
        return new Vector2(calculatedStyle.X, calculatedStyle.Y + calculatedStyle.Height);
    }

    public static Vector2 RightBottom(this CalculatedStyle calculatedStyle)
    {
        return new Vector2(calculatedStyle.X + calculatedStyle.Width, calculatedStyle.Y + calculatedStyle.Height);
    }

    public static float Right(this CalculatedStyle calculatedStyle)
    {
        return calculatedStyle.X + calculatedStyle.Width;
    }

    public static float Bottom(this CalculatedStyle calculatedStyle)
    {
        return calculatedStyle.Y + calculatedStyle.Height;
    }
}