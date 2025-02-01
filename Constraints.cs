namespace SilkyUIFramework;

public struct Constraints
{
    public float MinWidth { get; set; }
    public float MaxWidth { get; set; }
    public float MinHeight { get; set; }
    public float MaxHeight { get; set; }

    public readonly float ClampWidth(float width)
    {
        return MathHelper.Clamp(width, MinWidth, MaxWidth);
    }

    public readonly float ClampHeight(float height)
    {
        return MathHelper.Clamp(height, MinHeight, MaxHeight);
    }
}