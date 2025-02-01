namespace SilkyUIFramework;

public static class SilkyUIHelper
{
    public static Vector2 GetOriginalScreenSize() => PlayerInput.OriginalScreenSize;
    public static Vector2 GetUIScale() => new Vector2(Main.UIScale);

    /// <summary>
    /// 根据屏幕大小返回 <see cref="CalculatedStyle"/>
    /// </summary>
    public static CalculatedStyle GetBasicBodyDimensions()
    {
        var screenScaledSize = GetScreenScaledSize();
        return new CalculatedStyle(0f, 0f, screenScaledSize.X, screenScaledSize.Y);
    }

    public static Vector2 GetScreenScaledSize() => GetOriginalScreenSize() / GetUIScale();
}