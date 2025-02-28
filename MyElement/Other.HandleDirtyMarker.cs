namespace SilkyUIFramework;

public partial class Other
{
    public bool LayoutDirty { get; protected set; } = true;
    public bool PositionDirty { get; protected set; } = true;

    /// <summary>
    /// 标记脏
    /// </summary>
    public void MarkLayoutDirty()
    {
        if (LayoutDirty) return;

        LayoutDirty = true;
        Parent?.MarkLayoutDirty();
    }

    /// <summary>
    /// 处理脏标记 (应该在根元素调用)
    /// </summary>
    public void LayoutDirtyCheck()
    {
        if (LayoutDirty)
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
        if (PositionDirty)
        {
            ApplyPosition();
        }

        foreach (var child in _children)
        {
            child.PositionDirtyCheck();
        }
    }
}