namespace SilkyUIFramework.Core;

public enum Positioning
{
    Relative,
    Absolute,
    Sticky,
}

[Flags]
public enum StickyType
{
    Left = 0,
    Top = 1 << 0,
    Right = 1 << 1,
    Bottom = 1 << 2,
}