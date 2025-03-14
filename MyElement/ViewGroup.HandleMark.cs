using SilkyUIFramework.MyElement;

namespace SilkyUIFramework;

public partial class ViewGroup
{
    protected internal void OnChildDirty()
    {
        if (IsDirty) return;
        if (AutomaticWidth || AutomaticHeight)
        {
            IsDirty = true;
            Parent?.OnChildDirty();
            return;
        }
        IsDirty = true;
    }

    public override void UpdateBounds()
    {
        if (IsDirty)
        {
            var container = (Parent is null || Positioning is Core.Positioning.Fixed) ? DeviceHelper.GetViewportSizeByUIScale() : Parent.InnerBounds.Size;
            Measure(container);
            Trim(container);
            Layout();

            CleanupDirtyMark();
        }

        foreach (var child in Children)
        {
            child.UpdateBounds();
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