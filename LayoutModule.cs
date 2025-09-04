namespace SilkyUIFramework;

/// <summary>
/// 布局模型<br/>
/// 实现此接口的类型不应暴漏任何其他方法
/// </summary>
public abstract class LayoutModule(UIElementGroup parent)
{
    public readonly UIElementGroup Parent = parent;
    protected Size Gap;
    protected bool FitWidth, FitHeight;

    public virtual void UpdateCacheStatus()
    {
        Gap = Parent.Gap;
        FitWidth = Parent.FitWidth;
        FitHeight = Parent.FitHeight;
    }

    /// <summary>
    /// 通常在 FitWidth 或者 FitHeight 有效时，主动设定元素大小
    /// </summary>
    public virtual void PostPrepare() { }

    public virtual void ModifyAvailableSize(UIView view, int index, ref float? availableWidth, ref float? availableHeight) { }

    /// <summary>
    /// 通常用于统计子元素的一些信息
    /// </summary>
    public virtual void PostPrepareChildren() { }
    public virtual void PostRecalculateWidth() { }
    public virtual void PostRecalculateChildrenWidth() { }
    public virtual void PostResizeChildrenWidth() { }
    public virtual void PostRecalculateHeight() { }
    public virtual void PostRecalculateChildrenHeight() { }
    public virtual void PostResizeChildrenHeight() { }
    public virtual void ModifyLayoutOffset() { }

    #region SetBounds Methods

    /// <summary>
    /// 通常用于 OnPrepare 阶段直接设置 OuterBounds.Width
    /// </summary>
    protected static void SetInnerWidth(UIView target, float width)
    {
        target.SetInnerBoundsWidth(MathHelper.Clamp(width, target.MinInnerWidth, target.MaxInnerWidth));
    }

    /// <summary>
    /// 通常用于 OnPrepare 阶段直接设置 OuterBounds.Height
    /// </summary>
    protected static void SetInnerHeight(UIView target, float height)
    {
        target.SetInnerBoundsHeight(MathHelper.Clamp(height, target.MinInnerHeight, target.MaxInnerHeight));
    }

    /// <summary>
    /// 通常用于 OnResizeChildrenWidth 阶段直接设置 OuterBounds.Width
    /// </summary>
    protected static void SetOuterWidth(UIView target, float width)
    {
        target.SetOuterBoundsWidth(MathHelper.Clamp(width, target.MinOuterWidth, target.MaxOuterWidth));
    }

    /// <summary>
    /// 通常用于 OnResizeChildrenHeight 阶段直接设置 OuterBounds.Height
    /// </summary>
    protected static void SetOuterHeight(UIView target, float height)
    {
        target.SetOuterBoundsHeight(MathHelper.Clamp(height, target.MinOuterHeight, target.MaxOuterHeight));
    }

    #endregion
}