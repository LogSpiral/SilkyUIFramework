namespace SilkyUIFramework;

public partial class UIElementGroup
{
    protected internal virtual void NotifyParentChildDirty()
    {
        if (LayoutIsDirty) return;

        if (FitWidth || FitHeight)
        {
            LayoutIsDirty = true;
            PositionIsDirty = true;
            Parent?.NotifyParentChildDirty();
            return;
        }

        LayoutIsDirty = true;
        PositionIsDirty = true;
    }

    public override void RefreshLayout()
    {
        if (LayoutIsDirty)
        {
            if (Positioning.IsFree())
            {
                var container = GetParentAvailableSpace();
                Prepare(container.Width, container.Height);
                ResizeChildrenWidth();
                CalculateHeight();
                ResizeChildrenHeight();
                ApplyLayout();
            }
            else
            {
                PrepareChildren();
                ResizeChildrenWidth();
                MeasureChildrenHeight();
                ResizeChildrenHeight();
                ApplyLayout();
            }

            CleanupDirtyMark();
        }

        foreach (var child in GetValidChildren())
        {
            child.RefreshLayout();
        }
    }

    public override void UpdatePosition()
    {
        base.UpdatePosition();

        foreach (var child in GetValidChildren())
        {
            child.UpdatePosition();
        }
    }

    public override void RecalculatePosition()
    {
        base.RecalculatePosition();

        foreach (var child in LayoutChildren)
        {
            child.RecalculatePosition();
        }
    }
}