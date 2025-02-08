using SilkyUIFramework.Core;

namespace SilkyUIFramework;

public partial class Other
{
    public bool IsLayoutDirty { get; set; } = true;

    public bool IsPositionDirty { get; set; } = true;

    /// <summary>
    /// 冒泡标记脏
    /// </summary>
    public void BubbleMarkerDirty()
    {
        IsLayoutDirty = true;

        if (Positioning is Positioning.Absolute || !(Parent is { } parent)) return;

        parent.BubbleMarkerDirty();
    }

    /// <summary>
    /// 处理脏标记 (应该在根元素调用)
    /// </summary>
    public void LayoutDirtyCheck()
    {
        if (IsLayoutDirty)
        {
            Measure(Parent?._innerBounds.Size ?? Main.graphics.graphicsDevice.Viewport.Bounds.Size());
            Trim(Parent?._innerBounds.Size ?? Main.graphics.graphicsDevice.Viewport.Bounds.Size());
            Arrange();
        }

        foreach (var child in _children)
        {
            child.LayoutDirtyCheck();
        }
    }

    /// <summary>
    /// (应该在根元素调用)
    /// </summary>
    public void PositionDirtyCheck()
    {
        if (IsPositionDirty)
        {
            ApplyPosition(Parent?._innerBounds ?? Main.graphics.graphicsDevice.Viewport.Bounds, Parent?.ScrollOffset ?? Vector2.Zero);
        }

        foreach (var child in _children)
        {
            child.PositionDirtyCheck();
        }
    }
}