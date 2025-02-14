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

        if (Parent is not { } parent) return;

        parent.BubbleMarkerDirty();
    }

    /// <summary>
    /// 处理脏标记 (应该在根元素调用)
    /// </summary>
    public void LayoutDirtyCheck()
    {
        if (IsLayoutDirty)
        {
            var container = Parent is null ? GetViewportSize() : Parent._innerBounds.Size;
            Measure(container);
            Trim(container);
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
            ApplyPosition();
        }

        foreach (var child in _children)
        {
            child.PositionDirtyCheck();
        }
    }
}