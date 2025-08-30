namespace SilkyUIFramework;

/// <summary>
/// 布局模型<br/>
/// 实现此接口的类型不应暴漏任何其他方法
/// </summary>
public abstract class LayoutModel(UIElementGroup parent)
{
    protected UIElementGroup Parent { get; } = parent;
    protected bool FitWidth { get; set; }
    protected bool FitHeight { get; set; }

    protected virtual void UpdateCacheStatus()
    {
        FitWidth = Parent.FitWidth;
        FitHeight = Parent.FitHeight;
    }

    public abstract void OnPrepare();
    public abstract void OnPrepareChildren();
    public abstract void OnResizeChildrenWidth();
    public abstract void OnRecalculateHeight();
    public abstract void OnRecalculateChildrenHeight();
    public abstract void OnResizeChildrenHeight();
    public abstract void OnUpdateChildrenLayoutOffset();
}