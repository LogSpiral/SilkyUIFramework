namespace SilkyUIFramework;

public enum Positioning
{
    Relative,
    Absolute,
    Sticky,
    Fixed,
    Static,
}

[Flags]
public enum StickyType
{
    Left = 1 << 0,
    Top = 1 << 1,
    Right = 1 << 2,
    Bottom = 1 << 3,
    LeftTop = Left | Top,
    RightTop = Right | Top,
    LeftBottom = Left | Bottom,
    RightBottom = Right | Bottom,
}

public static class PositioningExtensions
{
    public static bool IsFree(this Positioning positioning) => positioning is Positioning.Fixed or Positioning.Absolute;
}