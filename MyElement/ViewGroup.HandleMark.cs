using SilkyUIFramework.MyElement;

namespace SilkyUIFramework;

public partial class ViewGroup
{
    protected internal void OnChildSizeDirty()
    {
        if (IsDirty) return;
        if (AutomaticWidth || AutomaticHeight) MarkDirty();
    }

    public void UpdateLayout()
    {
        if (IsDirty)
        {
            var container = Parent?.InnerBounds.Size ?? DeviceHelper.GetViewportSizeByUIScale();
            Measure(container);
            Trim(container);
            Layout();

            CleanupDirtyMark();
        }

        foreach (var child in Children.OfType<ViewGroup>())
        {
            child.UpdateLayout();
        }
    }

    public override void UpdatePosition()
    {
        base.UpdatePosition();

        foreach (var child in Children)
        {
            child.UpdatePosition();
        }
    }

    public override void RecalculatePosition()
    {
        base.RecalculatePosition();

        foreach (var child in Children)
        {
            child.RecalculatePosition();
        }
    }
}